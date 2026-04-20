namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <summary>
/// Describes how the target field value is materialized for a scenario.
/// </summary>
public enum ScenarioBodyKind
{
    /// <summary>
    /// The field is present in the dataset with a NULL value.<br/>
    /// Is used to test whether validation logic properly detects and handles NULL values<br/>
    /// according to the specification, depending on it, cases could be counted as positive or negative.
    /// </summary>
    NullLiteral,
    
    /// <summary>
    /// The field is present in the dataset with an empty string value.<br/>
    /// Is used to test whether validation logic properly detects and handles <c>string.Empty</c> values<br/>
    /// according to the specification, depending on it, cases could be counted as positive or negative.
    /// </summary>
    EmptyString,
    
    /// <summary>
    /// The field is present in the dataset with a single space character as its value.<br/>
    /// Is used to test whether validation logic properly detects and handles <c>" "</c> values<br/>
    /// according to the specification, depending on it, cases could be counted as positive or negative.
    /// </summary>
    SingleSpace,
    
    /// <summary>
    /// The field is present in the dataset with a tab character as its value.<br/>
    /// Is used to test whether validation logic properly detects and handles <c>"\t"</c> values<br/>
    /// according to the specification, depending on it, cases could be counted as positive or negative.
    /// </summary>
    Tab,
    
    /// <summary>
    /// The field is present in the dataset with a newline character as its value.<br/>
    /// Is used to test whether validation logic properly detects and handles <c>"\n"</c> values<br/>
    /// according to the specification, depending on it, cases could be counted as positive or negative.
    /// </summary>
    Newline,
    
    /// <summary>
    /// The field is present in the dataset with a specific literal value provided in the scenario descriptor.<br/>
    /// Applicable for any field and may be used to test various invalid values that do not conform to the specification rules.
    /// </summary>
    Literal,
    
    /// <summary>
    /// The field is completely missing from the dataset.<br/>
    /// Applicable only for required fields according to the specification,<br/>
    /// and may be used to test whether validation logic properly detects missing required fields.
    /// </summary>
    MissedField,
}