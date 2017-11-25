using System.ComponentModel;

namespace ioschoolsWebsite.Models.user
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