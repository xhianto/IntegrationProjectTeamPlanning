using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    /// <summary>
    /// Class <c> Response </c> models incoming MS Graph API calendar data
    /// </summary>
    /// <param name="value">a list with instances of Class <c> officeCalendar</c></param>
    public class Response
    {
        public List<OfficeCalendar> value { get; set; }  
    }
}
