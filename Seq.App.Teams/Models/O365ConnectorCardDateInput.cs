// <auto-generated /> Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Seq.App.Teams.Models
{
    using System.Linq;

    /// <summary>
    /// O365 connector card date input
    /// </summary>
    public partial class O365ConnectorCardDateInput : O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardDateInput class.
        /// </summary>
        public O365ConnectorCardDateInput() { }

        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardDateInput class.
        /// </summary>
        /// <param name="type">Input type name</param>
        /// <param name="id">Input Id. It must be unique per entire O365
        /// connector card.</param>
        /// <param name="isRequired">Define if this input is a required field.
        /// Default value is false.</param>
        /// <param name="title">Input title that will be shown as the
        /// placeholder</param>
        /// <param name="value">Default value for this input field</param>
        /// <param name="includeTime">Include time input field. Default value
        /// is false (date only).</param>
        public O365ConnectorCardDateInput(string type = default(string), string id = default(string), bool? isRequired = default(bool?), string title = default(string), string value = default(string), bool? includeTime = default(bool?))
            : base(type, id, isRequired, title, value)
        {
            IncludeTime = includeTime;
        }

        /// <summary>
        /// Gets or sets include time input field. Default value  is false
        /// (date only).
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "includeTime")]
        public bool? IncludeTime { get; set; }

    }
}
