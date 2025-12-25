namespace Marigergis.Attendance.WebApi.Models.Constants;

public class Roles
{
    public Roles()
    {
        var propertyList = new List<string>();
        var properties = GetType().GetProperties()
            // skip property List<string>
            .Where(prop => prop.PropertyType != typeof(List<string>));

        // Iterate all properties then add it to List<string> propertyList
        foreach (var property in properties)
        {
            var value = property.GetValue(this, null);
            if (value != null)
            {
                var stringValue = value.ToString();
                if (stringValue != null)
                {
                    propertyList.Add(stringValue);
                }
            }
        }

        // pass result to List<string> ArrayList
        ArrayList = propertyList;
    }

    public List<string> ArrayList { get; set; }
    public string Admin { get; set; } = "Admin";
    public string Employee { get; set; } = "Employee";
}
