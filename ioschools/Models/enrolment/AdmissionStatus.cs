using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.enrolment
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