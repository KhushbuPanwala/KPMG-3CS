using System;
using System.Collections.Generic;
using System.Text;

namespace KPMG3CS.DomainModel.Models.UserManagement
{
    public class Tanent : BaseModel
    {
        /// <summary>
        /// defines name of tanent
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// defines tanent start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// defines tanent end date
        /// </summary>
        public DateTime EndDate { get; set; }


        /// <summary>
        /// Defines tanent is soft deleted or not
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
