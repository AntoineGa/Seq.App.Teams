// <auto-generated /> Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Seq.App.Teams.Models
{
    using System.Linq;

    /// <summary>
    /// O365 connector card multiple choice input
    /// </summary>
    public partial class O365ConnectorCardMultichoiceInput : O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the
        /// O365ConnectorCardMultichoiceInput class.
        /// </summary>
        public O365ConnectorCardMultichoiceInput() { }

        /// <summary>
        /// Initializes a new instance of the
        /// O365ConnectorCardMultichoiceInput class.
        /// </summary>
        /// <param name="type">Input type name</param>
        /// <param name="id">Input Id. It must be unique per entire O365
        /// connector card.</param>
        /// <param name="isRequired">Define if this input is a required field.
        /// Default value is false.</param>
        /// <param name="title">Input title that will be shown as the
        /// placeholder</param>
        /// <param name="value">Default value for this input field</param>
        /// <param name="choices">Set of choices whose each item can be in any
        /// subtype of O365ConnectorCardMultichoiceInputChoice.</param>
        /// <param name="style">Choice item rendering style. Default valud is
        /// 'compact'. Possible values include: 'compact', 'expanded'</param>
        /// <param name="isMultiSelect">Define if this input field allows
        /// multiple selections. Default value is false.</param>
        public O365ConnectorCardMultichoiceInput(string type = default(string), string id = default(string), bool? isRequired = default(bool?), string title = default(string), string value = default(string), System.Collections.Generic.IList<O365ConnectorCardMultichoiceInputChoice> choices = default(System.Collections.Generic.IList<O365ConnectorCardMultichoiceInputChoice>), string style = default(string), bool? isMultiSelect = default(bool?))
            : base(type, id, isRequired, title, value)
        {
            Choices = choices;
            Style = style;
            IsMultiSelect = isMultiSelect;
        }

        /// <summary>
        /// Gets or sets set of choices whose each item can be in any subtype
        /// of O365ConnectorCardMultichoiceInputChoice.
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "choices")]
        public System.Collections.Generic.IList<O365ConnectorCardMultichoiceInputChoice> Choices { get; set; }

        /// <summary>
        /// Gets or sets choice item rendering style. Default valud is
        /// 'compact'. Possible values include: 'compact', 'expanded'
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "style")]
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets define if this input field allows multiple
        /// selections. Default value is false.
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "isMultiSelect")]
        public bool? IsMultiSelect { get; set; }

    }
}
