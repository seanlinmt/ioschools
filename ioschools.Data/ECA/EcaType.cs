using System.ComponentModel;

namespace ioschools.Data.ECA
{
    public enum EcaType
    {
        [Description("Sports/Games/Competition")]
        SPORTS,
        [Description("Clubs/Society")]
        CLUBS,
        [Description("Other Duties")]
        DUTIES,
        [Description("Uniform Bodies")]
        UNIFORM
    }
}
