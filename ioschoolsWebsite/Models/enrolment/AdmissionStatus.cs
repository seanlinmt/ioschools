using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.enrolment
{
    public enum AdmissionStatus
    {
        NOID,
        DUPLICATEEMAIL,
        NOEMAIL,
        UNKNOWN,
        SUCCESS,
        INCORRECT_NRIC_PASSPORT
    }
}