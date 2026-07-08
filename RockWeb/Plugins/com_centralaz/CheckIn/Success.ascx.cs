// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Web.UI;

using com.centralaz.CheckInLabels;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Success")]
    [Category("com_centralaz > Check-in")]
    [Description("Displays the details of a successful checkin.")]
    [LinkedPage("Person Select Page")]
    public partial class Success : CheckInBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RockPage.AddScriptLink("~/Scripts/CheckinClient/cordova-2.4.0.js", false);
            RockPage.AddScriptLink("~/Scripts/CheckinClient/ZebraPrint.js");

            RockPage.AddScriptLink("~/Scripts/iscroll.js");
            RockPage.AddScriptLink("~/Scripts/CheckinClient/checkin-core.js");
            RockPage.AddScriptLink("~/Plugins/com_centralaz/CheckIn/Scripts/checkin-core.js");
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (CurrentWorkflow == null || CurrentCheckInState == null)
            {
                NavigateToHomePage();
            }
            else
            {
                if (!Page.IsPostBack)
                {
                    try
                    {

                        // Print the labels
                        foreach (var family in CurrentCheckInState.CheckIn.Families.Where(f => f.Selected))
                        {
                            lbAnother.Visible =
                                CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Individual &&
                                family.People.Count > 1;

                            foreach (var person in family.GetPeople(true))
                            {
                                foreach (var groupType in person.GetGroupTypes(true))
                                {
                                    phResults.Controls.Add(new LiteralControl("<br/>Sort through all the checkin entities and start tring to print labels by group type"));
                                    foreach (var group in groupType.GetGroups(true))
                                    {
                                        foreach (var location in group.GetLocations(true))
                                        {
                                            foreach (var schedule in location.GetSchedules(true))
                                            {
                                                var li = new HtmlGenericControl("li");
                                                li.InnerText = string.Format("{0} : {2} at {3}",
                                                    person.ToString(), group.ToString(), location.ToString(), schedule.ToString(), person.SecurityCode);

                                                phResults.Controls.Add(li);
                                            }
                                        }
                                    }

                                    phResults.Controls.Add(new LiteralControl("<br/>For each group type's set of labels, check if they print from the client or the server"));
                                    try
                                    {
                                        var printFromClient = groupType.Labels.Where(l => l.PrintFrom == Rock.Model.PrintFrom.Client).OrderBy(l => l.Order);
                                        var pfcString = printFromClient.ToJson();
                                        phResults.Controls.Add(new LiteralControl("<br/>Checking for labels to print from client." + pfcString));
                                        if (printFromClient.Any())
                                        {
                                            phResults.Controls.Add(new LiteralControl("<br/>Print from client labels detected."));
                                            var urlRoot = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
                                            printFromClient.ToList().ForEach(l => l.LabelFile = urlRoot + l.LabelFile);
                                            AddLabelScript(printFromClient.ToJson());
                                        }

                                        var printFromServer = groupType.Labels.Where(l => l.PrintFrom == Rock.Model.PrintFrom.Server).OrderBy(l => l.Order);
                                        var pfsString = printFromServer.Any();
                                        phResults.Controls.Add(new LiteralControl("<br/>Checking for labels to print from server: " + pfsString));
                                        if (printFromServer.Any())
                                        {
                                            phResults.Controls.Add(new LiteralControl("<br/>Print from server labels detected."));
                                            PrintFromServerLabels(person, groupType, printFromServer);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        phResults.Controls.Add(new LiteralControl(string.Format("<br/><span class='text-danger '>Could not connect to printer! {0}</span>", ex.Message)));

                                        // Problem printing person's labels.
                                        LogException(ex);
                                    }
                                }
                            }
                        }


                        /*
                        <table class="table table-condensed">
                            <tr>
                                <th></th><th>4:30</th><th>6:30</th><th></th>
                            </tr>
                            <tr>
                                <td>Noah</td><td class="bg-success">B131</td><td class="bg-danger text-danger">error</td>
                            </tr>
                            <tr>
                                <td>Alex</td><td class="bg-success">B131</td><td>B129</td><td><!--no error--></td>
                            </tr>
                        </table>
                        */


                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }
            }
        }


        /// <summary>
        /// Prints the labels that are the "from server" ones.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="printFromServer">The print from server.</param>
        private void DrawFromServerLabels(CheckInPerson person, CheckInGroupType groupType, IEnumerable<CheckInLabel> printFromServer)
        {

            int numOfLabels = printFromServer.Count();
            int labelIndex = 0;
            foreach (var label in printFromServer.OrderBy(l => l.Order))
            {
                labelIndex++;
                var labelCache = KioskLabel.Get(label.FileGuid);

                // There is no printer set up to print from server, so print to the page instead
                string printContent = labelCache.FileContent;
                // This is documented in <\IT\Projects\Rock RMS\CustomProjects\Check-in\Rock Central Check-in Setup and Design.docx>
                if (printContent.StartsWith("Assembly:"))
                {
                    // New Method Here
                    LoadPrintLabelAndDraw(printContent, label, CurrentCheckInState, person, groupType);
                }
            }
        }


        /// <summary>
        /// Prints the labels that are the "from server" ones.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="printFromServer">The print from server.</param>
        private void PrintFromServerLabels(CheckInPerson person, CheckInGroupType groupType, IEnumerable<CheckInLabel> printFromServer)
        {
            Socket socket = null;
            bool hasCutter = true;
            string currentIp = string.Empty;
            int numOfLabels = printFromServer.Count();
            int labelIndex = 0;
            foreach (var label in printFromServer.OrderBy(l => l.Order))
            {
                phResults.Controls.Add(new LiteralControl("<br/>Run through the labels to print."));
                labelIndex++;
                var labelCache = KioskLabel.Get(label.FileGuid);
                if (labelCache != null)
                {
                    phResults.Controls.Add(new LiteralControl("<br/>Check to see if the Printer Address exists."));
                    phResults.Controls.Add(new LiteralControl(string.Format("<br/>Printer Address: {0}", label.PrinterAddress)));
                    if (!string.IsNullOrWhiteSpace(label.PrinterAddress))
                    {
                        phResults.Controls.Add(new LiteralControl("<br/>Check to see if the Printer Address is empty."));
                        if (label.PrinterAddress != currentIp)
                        {
                            if (socket != null && socket.Connected)
                            {
                                socket.Shutdown(SocketShutdown.Both);
                                socket.Close();
                            }

                            currentIp = label.PrinterAddress;
                            var printerIp = new IPEndPoint(IPAddress.Parse(currentIp), 9100);
                            phResults.Controls.Add(new LiteralControl(string.Format("<br/>Printer IpEndpoint: {0}", printerIp)));
                            var deviceId = label.PrinterDeviceId;
                            phResults.Controls.Add(new LiteralControl(string.Format("<br/>Printer Device Id: {0}", deviceId)));
                            hasCutter = GetPrinterCutterOption(deviceId);

                            phResults.Controls.Add(new LiteralControl("<br/>Create a new socket."));
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            IAsyncResult result = socket.BeginConnect(printerIp, null, null);
                            bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                            phResults.Controls.Add(new LiteralControl(string.Format("<br/>Socket success status: {0}", success)));
                        }

                        phResults.Controls.Add(new LiteralControl("<br/>Get the printContent."));
                        string printContent = labelCache.FileContent;
                        // This is documented in <\IT\Projects\Rock RMS\CustomProjects\Check-in\Rock Central Check-in Setup and Design.docx>
                        phResults.Controls.Add(new LiteralControl("<br/>Check to see if its a c# label"));
                        if (printContent.StartsWith("Assembly:"))
                        {

                            if (socket != null && socket.Connected)
                            {
                                socket.Shutdown(SocketShutdown.Both);
                                socket.Close();
                            }
                            phResults.Controls.Add(new LiteralControl("<br/>Handle c# label now."));
                            LoadPrintLabelAndPrint(printContent, label, CurrentCheckInState, person, groupType);
                        }
                        else
                        {
                            phResults.Controls.Add(new LiteralControl("<br/>Not a c# label, so resolve the merge fields"));
                            foreach (var mergeField in label.MergeFields)
                            {
                                if (!string.IsNullOrWhiteSpace(mergeField.Value))
                                {
                                    printContent = Regex.Replace(printContent, string.Format(@"(?<=\^FD){0}(?=\^FS)", mergeField.Key), ZebraFormatString(mergeField.Value));
                                }
                                else
                                {
                                    // Remove the box preceding merge field
                                    printContent = Regex.Replace(printContent, string.Format(@"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key), string.Empty);
                                    // Remove the merge field
                                    printContent = Regex.Replace(printContent, string.Format(@"\^FD{0}\^FS", mergeField.Key), "^FD^FS");
                                }
                            }

                            // Inject the cut command on the last label (if the printer is has a cutter)
                            // otherwise supress the backfeed (^XB)
                            if (labelIndex == numOfLabels && hasCutter)
                            {
                                printContent = Regex.Replace(printContent.Trim(), @"\" + @"^PQ1,0,1,Y", string.Empty);
                                printContent = Regex.Replace(printContent.Trim(), @"\" + @"^MMT", @"^MMC");
                            }
                            else
                            {
                                printContent = Regex.Replace(printContent.Trim(), @"\" + @"^XZ$", @"^XB^XZ");
                            }

                            phResults.Controls.Add(new LiteralControl("<br/>Try to get the bytes together with the socket"));
                            if (socket.Connected)
                            {
                                var ns = new NetworkStream(socket);
                                byte[] toSend = System.Text.Encoding.ASCII.GetBytes(printContent);
                                ns.Write(toSend, 0, toSend.Length);
                            }
                            else
                            {
                                phResults.Controls.Add(new LiteralControl("<br/>NOTE: Could not connect to printer!"));
                            }
                        }


                    }
                    //If all else fails, print the labels to the screen
                    else
                    {

                        //throw new Exception("The printer address was empty");

                        ////Try to draw the label to the screen.
                        var printFS = groupType.Labels.Where(l => l.PrintFrom == Rock.Model.PrintFrom.Server).OrderBy(l => l.Order);
                        if (printFS.Any())
                        {
                            DrawFromServerLabels(person, groupType, printFS);
                        }

                    }
                }

                // labelCache != null
            }

            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }



        /// <summary>
        /// Gets the printer cutter option from either a "HasCutter" (boolean) attribute
        /// on the printer device or the words "w/Cutter" in the printer's Description.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>true if printer has a cutter; false otherwise</returns>
        protected bool GetPrinterCutterOption(int? deviceId)
        {
            bool hasCutter = false;

            // Get the device from cache
            var currentGroupTypeIds = (Session["CheckInGroupTypeIds"] != null) ? Session["CheckInGroupTypeIds"] as List<int> : new List<int>();
            KioskDevice kioskDevice = KioskDevice.Get(deviceId.GetValueOrDefault(), currentGroupTypeIds);
            hasCutter = kioskDevice.Device.GetAttributeValue("HasCutter").AsBoolean();

            // also check the Description for the w/Cutter keywords
            if (!hasCutter)
            {
                hasCutter = Regex.IsMatch(kioskDevice.Device.Description, "w/Cutter", RegexOptions.IgnoreCase);
            }

            return hasCutter;
        }

        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDone_Click(object sender, EventArgs e)
        {
            NavigateToHomePage();
        }

        private string ZebraFormatString(string input, bool isJson = false)
        {
            if (isJson)
            {
                return input.Replace("é", @"\\82");  // fix acute e
            }
            else
            {
                return input.Replace("é", @"\82");  // fix acute e
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAnother control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAnother_Click(object sender, EventArgs e)
        {
            if (KioskCurrentlyActive)
            {
                foreach (var family in CurrentCheckInState.CheckIn.Families.Where(f => f.Selected))
                {
                    foreach (var person in family.People.Where(p => p.Selected))
                    {
                        person.Selected = false;

                        foreach (var groupType in person.GroupTypes.Where(g => g.Selected))
                        {
                            groupType.Selected = false;
                        }
                    }
                }

                SaveState();
                NavigateToLinkedPage("PersonSelectPage");

            }
            else
            {
                NavigateToHomePage();
            }
        }

        //# region Helper Methods
        private void LoadPrintLabelAndDraw(string assemblyString, CheckInLabel label, CheckInState checkInState, CheckInPerson person, CheckInGroupType groupType)
        {

            try
            {
                //Get a list of all the labels as separate bitmaps
                List<Bitmap> bitmaps = new List<Bitmap>();
                string line1 = assemblyString.Split(new[] { '\r', '\n' }).FirstOrDefault();
                // Remove the "Assembly:" prefix
                var assemblyParts = line1.ReplaceCaseInsensitive("Assembly:", "").Trim().Split(',');
                var assemblyName = assemblyParts[0];
                var assemblyClass = assemblyParts[1];
                var assemblyDrawingClass = assemblyParts[2];

                var drawLabel = DrawLabelHelper.GetDrawLabelClass(assemblyName, assemblyDrawingClass);
                bitmaps = drawLabel.Draw(label, person, checkInState, groupType);

                for (var i = 0; i < bitmaps.Count; i++)
                {

                    using (MemoryStream ms = new MemoryStream())
                    {

                        string base64String = null;
                        Bitmap bitmap = bitmaps[i];
                        // Save the bitmap to the stream in a web-friendly format (PNG or JPEG)

                        bitmap.Save(ms, ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();

                        // Convert the byte array to a base64 string
                        base64String = Convert.ToBase64String(imageBytes);
                        // Inject into the literal control as an HTML image source
                        phResults.Controls.Add(new LiteralControl(string.Format("<img src='data:image/png;base64,{0}' id='{1}' alt='Bitmap Image' />", base64String, ("bitmap" + i).ToString())));
                    }
                }


            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }


        private void LoadPrintLabelAndPrint(string assemblyString, CheckInLabel label, CheckInState checkInState, CheckInPerson person, CheckInGroupType groupType)
        {
            // Use only the first line
            string line1 = assemblyString.Split(new[] { '\r', '\n' }).FirstOrDefault();
            // Remove the "Assembly:" prefix
            var assemblyParts = line1.ReplaceCaseInsensitive("Assembly:", "").Trim().Split(',');
            var assemblyName = assemblyParts[0];
            var assemblyClass = assemblyParts[1];

            var printLabel = PrintLabelHelper.GetPrintLabelClass(assemblyName, assemblyClass);
            phResults.Controls.Add(new LiteralControl("<br/>Reached LoadPrintLabelAndPrint"));
            phResults.Controls.Add(new LiteralControl("<br/>Now the code goes back into the compiled code to get the label provider and set."));
            printLabel.Print(label, person, checkInState, groupType);
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript(string jsonObject)
        {
            string script = string.Format(@"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
			printLabels();
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels.' + error[0],  // message
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
", ZebraFormatString(jsonObject, true));
            ScriptManager.RegisterStartupScript(this, this.GetType(), "addLabelScript", script, true);
        }

        //#endregion
    }

}