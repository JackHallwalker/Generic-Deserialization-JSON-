namespace Generic_Deserialization_JSON.Models
{
    public class MockModel
    {
        public int PropertyA { get; set; }

        public double PropertyB { get; set; }

        public List<SubClass>? PropertyC { get; set; }
    }

    public class SubClass
    {
        public string SubPropertyA { get; set; } = string.Empty;
        public object? SubPropertyB { get; set; }
    }
}
