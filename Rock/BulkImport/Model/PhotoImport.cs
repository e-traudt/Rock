namespace Rock.BulkImport.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.BulkImport.Model.PhotoImport" />
    [Rock.Data.RockClientInclude( "PhotoImport Model for Rock Bulk Insert APIs" )]
    public class PhotoImport
    {
        /// <summary>
        /// Gets or sets the type of the photo.
        /// </summary>
        /// <value>
        /// The type of the photo.
        /// </value>
        [Rock.Data.RockClientInclude( "Person=1, Family=2" )]
        public PhotoImportType PhotoType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public enum PhotoImportType
        {
            Person = 1,
            Family = 2
        }

        /// <summary>
        /// Gets or sets the person foreign identifier.
        /// </summary>
        /// <value>
        /// The person foreign identifier.
        /// </value>
        public int ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the MIME.
        /// </summary>
        /// <value>
        /// The type of the MIME.
        /// </value>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the photo data as b
        /// </summary>
        /// <value>
        /// The photo data.
        /// </value>
        [Rock.Data.RockClientInclude( "Set PhotoData using Base64 format" )]
        public string PhotoData { get; set; }
    }
}
