using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using ioschools.Library.Helpers;

namespace ioschools.Models.discipline
{
    // IMPORTANT: CHANGE MAY BREAK ISMERIT() METHOD BELOW. ADD MERITS BEFORE ALL AND DEMERITS AFTER ALL
    public enum DisciplineType
    {
        [Description("+Honesty")]
        Honesty = 1,
        [Description("+Initiative")]
        Initiative = 2,
        [Description("+Diligence/Completion/Neat-ness in school work")]
        Diligence = 3,
        [Description("+Tidiness/Neatness in appearance and classroom")]
        Tidiness = 4,
        [Description("+Kindness")]
        Kindness = 5,
        [Description("+Helpfulness")]
        Helpfulness = 6,
        [Description("+Reports misdeeds without fear/favour")]
        WithoutFear = 7,
        [Description("+Willing to take responsibilities")]
        Responsible = 8,
        [Description("+Courageous acts")]
        Courageous = 9,
        [Description("+Other good deeds")]
        OtherGood = 10,
        [Description("+Positive changes in attitude/behaviour/performance")]
        PositiveChanges = 11,
        [Description("-Littering")]
        Littering = 12,
        [Description("-Spitting")]
        Spitting = 13,
        [Description("-Shabbiness")]
        Shabbiness = 14,
        [Description("-Rowdiness")]
        Rowdiness = 15,
        [Description("-Coming to school/lessons late")]
        Late = 16,
        [Description("-Failure to complete/Hand in assignment")]
        FailureToCompleteAssignment = 17,
        [Description("-Eating in class")]
        EatingInClass = 18,
        [Description("-Running along corridor")]
        RunningAlongCorridor = 19,
        [Description("-Playing rough")]
        PlayingRough = 20,
        [Description("-Uttering foul language")]
        Language = 21,
        [Description("-Telling lies/Spreading rumours")]
        Lies = 22,
        [Description("-Quarrelling")]
        Quarrelling = 23,
        [Description("-Vandalism")]
        Vandalism = 24,
        [Description("-Stealing")]
        Stealing = 25,
        [Description("-Disrespect to staff/prefect")]
        Disrespect = 26,
        [Description("-Fighting")]
        Fighting = 27,
        [Description("-Bullying")]
        Bullying = 28,
        [Description("-In possession of prohibited items")]
        ProhibitedItems = 29,
        [Description("-Leaving class/school without permission")]
        Truant = 30,
        [Description("-Gambling/Betting")]
        Gambling = 31,
        [Description("-Seen in company of outsiders of suspicious character")]
        Dodgy = 32,
        [Description("-Inappropriate/Indecent behaviour")]
        Indecent = 33,
        [Description("-Smoking")]
        Smoking = 34,
        [Description("-Cheating in School Test/Examination")]
        Cheating = 35,
        [Description("-Drug abuse")]
        DrugAbuse = 36,
        [Description("-Any other inappropriate conduct/behaviour")]
        OtherBad = 37
    }

    public static class DisciplineTypeHelper
    {
        public static readonly List<SelectListItem> Types;
        static DisciplineTypeHelper()
        {
            Types = Enum.GetValues(typeof (DisciplineType))
                .Cast<Enum>()
                .Select(x => new SelectListItem()
                                 {
                                     Text = x.ToDescriptionString(),
                                     Value = x.ToInt().ToString()
                                 }).ToList();

            Types.Insert(0, new SelectListItem() {Text = "Select type ...", Value = ""});
        }

        public static bool IsMerit(this DisciplineType row)
        {
            return row <= DisciplineType.PositiveChanges;
        }
    }
}
