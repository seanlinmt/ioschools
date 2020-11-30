using System.ComponentModel;

namespace ioschools.Models.user
{
    public enum Designation
    {
        [Description("Mdm.")]
        MADAM,
        [Description("Mr.")]
        MR,
        [Description("Mrs.")]
        MRS,
        [Description("Ms.")]
        MS,
        [Description("Datuk")]
        DATUK,
        [Description("Dato")]
        DATO,
        [Description("Datin")]
        DATIN,
        [Description("Dr.")]
        DR,
        
    }
}