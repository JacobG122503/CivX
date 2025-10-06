public class SaveData
{
    public List<Human> Humans { get; set; } = new List<Human>();
    public DateTime SaveTime { get; set; }
    public int CurrentYear { get; set; } = DateTime.Now.Year;
}
