﻿// <copyright>
// Copyright by the Central Christian Church
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
using System.Text.RegularExpressions;
using com.centralaz.RoomManagement.Model;
using Rock.Extension;
using Rock.Model;

namespace com.centralaz.RoomManagement.ReportTemplates
{
    /// <summary>
    /// Base class for report templates (i.e. Default, Lava, etc) 
    /// </summary>
    public abstract class ReportTemplate : Component
    {
        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public abstract List<Exception> Exceptions { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public abstract byte[] GenerateReport( List<ReservationService.ReservationSummary> reservationSummaryList, string logoFileUrl, string font, DateTime? filterStartDate, DateTime? filterEndDate, string lavaTemplate = "" );
    }
}
