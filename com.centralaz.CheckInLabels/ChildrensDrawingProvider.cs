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
using System.Linq;
using System.Text;
using System.Web;
using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using System.Drawing;
using System.Drawing.Imaging;
using Rock.Address;


namespace com.centralaz.CheckInLabels
{
    /// <summary>
    /// This is a port from our original pre-arena print label system which
    /// has been adapted to work with Rock's Label Merge Fields.  It will look for
    /// and use the following merge code keys in the Zebra .PRN file so they can be mapped
    /// to Rock's Label Merge Fields:
    /// * CentralAZ.BirthdayImageFile (whose reference should point to a .BMP of a Birthday cake icon)
    /// * CentralAZ.ClaimCardFooter
    /// * CentralAZ.ClaimCardTitle
    /// * CentralAZ.HealthNotesTitle
    /// * CentralAZ.ParentsInitialsTitle
    /// * CentralAZ.ServicesLabel
    /// * AllergyNote
    /// * SpecialNeedsIntakeFlag
    /// * InfoIconFile (whose reference should point to a .BMP of a Info icon)
    /// * EpiPenFlag
    /// * SelfCheckOutFlag
    /// * LegalNote
    /// </summary>
    /// <seealso cref="com.centralaz.CheckInLabels.IDrawLabel" />
    internal class ChildrensDrawingProvider : IDrawLabel
    {
        private RockContext rockContext = new RockContext();
        private ChildrenLabelSet label;

        public ChildrensDrawingProvider()
        {
        }

        /// <summary>
        /// IPrintLabel implementation to print out name tags.
        /// </summary>
        //public Graphics Draw( Rock.CheckIn.CheckInLabel checkInLabel, CheckInPerson person, CheckInState checkInState, CheckInGroupType groupType)
        //public void Draw( Rock.CheckIn.CheckInLabel checkInLabel, CheckInPerson person, CheckInState checkInState, CheckInGroupType groupType, Graphics g )
        public List<Bitmap> Draw( Rock.CheckIn.CheckInLabel checkInLabel, CheckInPerson person, CheckInState checkInState, CheckInGroupType groupType)
        {
            
            //Graphics g = null;

                IEnumerable <Rock.CheckIn.CheckInLabel> printFromServer = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server );
            //if ( printFromServer.Any() )
            //{
                
                    InitLabel( checkInLabel, person, checkInState, groupType );
                    
                
            //}

            return label.DrawAllLabels();
            //return g;

        }

        /// <summary>
        /// Intialize a person's label set with the information from the given person, occurrence(s),
        /// and attendance record.
        /// </summary>
        private void InitLabel( Rock.CheckIn.CheckInLabel checkInLabel, CheckInPerson attendee, CheckInState checkInState, CheckInGroupType groupType )
        {
            CheckInLocation firstLocation = null;
            label = new ChildrenLabelSet
            {
                FirstName = attendee.Person.NickName.Trim() != string.Empty ? attendee.Person.NickName : attendee.Person.FirstName,
                LastName = attendee.Person.LastName,
                FullName = string.Format( "{0} {1}", attendee.Person.NickName, attendee.Person.LastName ),
                BirthdayDate = attendee.Person.BirthDate ?? DateTime.MinValue,
                SecurityToken = attendee.SecurityCode,
                CheckInDate = DateTime.Now
            };

            label.AttendanceLabelTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.AttendanceLabelTitle" ) ? checkInLabel.MergeFields["CentralAZ.AttendanceLabelTitle"] : string.Empty;
            label.BirthdayImageFile = checkInLabel.MergeFields.ContainsKey( "CentralAZ.BirthdayImageFile" ) ? checkInLabel.MergeFields["CentralAZ.BirthdayImageFile"] : string.Empty;
            //label.BirthdayImageFile = "C:\\Users\\kate\\source\\com_9embers\\Plugins\\v16.13\\Rock\\RockWeb\\Content\\InternalSite\\Check-in\\cake2.bmp";
            label.Footer = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ClaimCardFooter" ) ? checkInLabel.MergeFields["CentralAZ.ClaimCardFooter"] : string.Empty;
            label.ClaimCardTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ClaimCardTitle" ) ? checkInLabel.MergeFields["CentralAZ.ClaimCardTitle"] : string.Empty;
            label.HealthNotesTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.HealthNotesTitle" ) ? checkInLabel.MergeFields["CentralAZ.HealthNotesTitle"] : string.Empty;
            label.HealthNotes = checkInLabel.MergeFields.ContainsKey( "AllergyNote" ) ? checkInLabel.MergeFields["AllergyNote"] : string.Empty;
            label.HealthNoteFlag = !string.IsNullOrWhiteSpace( label.HealthNotes );
            label.ParentsInitialsTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ParentsInitialsTitle" ) ? checkInLabel.MergeFields["CentralAZ.ParentsInitialsTitle"] : string.Empty;
            label.ServicesTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ServicesLabel" ) ? checkInLabel.MergeFields["CentralAZ.ServicesLabel"] : string.Empty;

            // Get start times from any selected schedules...
            // This section is only needed because we have a weird "Transfer: " chunk
            // on the label that lists all the services the person is checked into.
            StringBuilder services = new StringBuilder();
            foreach ( var group in groupType.GetGroups( true ) )
            {
                //foreach ( var location in group.Locations.Where( l => l.Selected ).OrderBy( e => e.Schedules.Min( s => s.StartTime ) ) )
                foreach ( var location in group.GetLocations( true ).OrderBy( e => e.Schedules.Min( s => s.StartTime ) ) )
                {
                    // Put the first location's name on the label
                    if ( firstLocation == null )
                    {
                        firstLocation = location;
                        label.RoomName = firstLocation.Location.Name;
                    }

                    foreach ( var schedule in location.GetSchedules( true ) )
                    {
                        if ( services.Length > 0 )
                        {
                            services.Append( ", " );
                        }
                        services.Append( schedule.StartTime.Value.ToShortTimeString() );
                    }
                }
            }

            label.Services = services.ToString();
            SetAgeGroup( attendee.Person );

            label.PhotoFlag = checkInLabel.MergeFields.ContainsKey( "PhotoFlag" ) ? Convert.ToBoolean( checkInLabel.MergeFields["PhotoFlag"] ) : false;
            label.PhotoPermissionImageFile = checkInLabel.MergeFields.ContainsKey( "PhotoPermissionFile" ) ? checkInLabel.MergeFields["PhotoPermissionFile"] : string.Empty;

            //label.InfoIconFile = "C:\\Users\\kate\\source\\com_9embers\\Plugins\\v16.13\\Rock\\RockWeb\\Content\\InternalSite\\Check-in\\PhotoPermission4.bmp";
            label.SpecialNeedsIntakeFlag = checkInLabel.MergeFields.ContainsKey( "SpecialNeedsIntakeFlag" ) ? Convert.ToBoolean(checkInLabel.MergeFields["SpecialNeedsIntakeFlag"]) : false;      
            label.InfoIconFile = checkInLabel.MergeFields.ContainsKey( "InfoIconFile" ) ? checkInLabel.MergeFields["InfoIconFile"] : string.Empty;
            //label.InfoIconFile = "C:\\Users\\kate\\source\\com_9embers\\Plugins\\v16.13\\Rock\\RockWeb\\Content\\InternalSite\\Check-in\\info.bmp";
            label.EpiPenFlag = checkInLabel.MergeFields.ContainsKey( "EpiPenFlag" ) ? Convert.ToBoolean( checkInLabel.MergeFields["EpiPenFlag"]) : false;
            label.SelfCheckOutFlag = checkInLabel.MergeFields.ContainsKey( "SelfCheckOutFlag" ) ? Convert.ToBoolean( checkInLabel.MergeFields["SelfCheckOutFlag"]) : false;
            label.LegalNoteFlag = checkInLabel.MergeFields.ContainsKey( "LegalNote" ) && !string.IsNullOrWhiteSpace( checkInLabel.MergeFields["LegalNote"] ) ? true : false;
        }

        /// <summary>
        /// This sets the label's "Age Group" value to a specific text (as per Children's
        /// ministries) based on the person's age and or grade.
        /// Basically:
        ///		* if the person is under the age of 2, display age in months;
        ///     * if they are in grade school, display the grade
        ///		* otherwise display age in years.
        /// </summary>
        /// <param name="person">The person whose label is being created.</param>
        private void SetAgeGroup( Person person )
        {
            if ( person.Age.HasValue && person.Age < 2 )
            {
                label.AgeGroup = String.Format( "{0} months", GetAgeInMonths( person.BirthDate.Value ) );
            }
            else
            {
                label.AgeGroup = ( !string.IsNullOrEmpty( person.GradeFormatted ) ) ? person.GradeFormatted : String.Format( "{0} year olds", person.Age );
            }
        }

        /// <summary>
        /// Calculates the age in whole months for the given birthday.
        /// </summary>
        /// <param name="birthday">A birthdate</param>
        /// <returns>number of whole months</returns>
        private static int GetAgeInMonths( DateTime birthday )
        {
            return GetDeltaInMonths( birthday, DateTime.Now );
        }

        /// <summary>
        /// An algorthim to determine the number of whole months
        /// between two dates (by Greg Golden).
        /// </summary>
        /// <param name="then">A date.</param>
        /// <param name="now">Another date.</param>
        /// <returns>number of whole months between the two dates</returns>
        private static int GetDeltaInMonths( DateTime then, DateTime now )
        {
            int thenMonthsSince1900 = 12 * ( then.Year - 1900 ) + then.Month;
            int nowMonthsSince1900 = 12 * ( now.Year - 1900 ) + now.Month;

            int monthsOld = nowMonthsSince1900 - thenMonthsSince1900;

            if ( now.Day < then.Day )
            {
                monthsOld--;
            }

            return monthsOld;
        }

        
    }
}
