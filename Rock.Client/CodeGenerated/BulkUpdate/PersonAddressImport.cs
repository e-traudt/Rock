//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright by the Spark Development Network
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


namespace Rock.Client.BulkUpdate
{
    /// <summary>
    /// Model for Rock Bulk Insert APIs
    /// </summary>
    public partial class PersonAddressImportEntity
    {
        /// <summary />
        public string City { get; set; }

        /// <summary />
        public string Country { get; set; }

        /// <summary />
        public string County { get; set; }

        /// <summary />
        public int GroupLocationTypeValueId { get; set; }

        /// <summary />
        public bool IsMailingLocation { get; set; }

        /// <summary />
        public bool IsMappedLocation { get; set; }

        /// <summary />
        public double? Latitude { get; set; }

        /// <summary />
        public double? Longitude { get; set; }

        /// <summary />
        public string PostalCode { get; set; }

        /// <summary />
        public string State { get; set; }

        /// <summary />
        public string Street1 { get; set; }

        /// <summary />
        public string Street2 { get; set; }

        /// <summary>
        /// Copies the base properties from a source PersonAddressImport object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( PersonAddressImport source )
        {
            this.City = source.City;
            this.Country = source.Country;
            this.County = source.County;
            this.GroupLocationTypeValueId = source.GroupLocationTypeValueId;
            this.IsMailingLocation = source.IsMailingLocation;
            this.IsMappedLocation = source.IsMappedLocation;
            this.Latitude = source.Latitude;
            this.Longitude = source.Longitude;
            this.PostalCode = source.PostalCode;
            this.State = source.State;
            this.Street1 = source.Street1;
            this.Street2 = source.Street2;

        }
    }

    /// <summary>
    /// Model for Rock Bulk Insert APIs
    /// </summary>
    public partial class PersonAddressImport : PersonAddressImportEntity
    {
    }
}
