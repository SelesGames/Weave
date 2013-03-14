using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weave
{
    /// <summary>
    /// Stores user-info that is saved locally on the phone
    /// </summary>
    public class UserInfo
    {
        public Guid Id { get; set; }
        public bool IsSavedToCloud { get; set; }

        public static UserInfo CreateNewUser()
        {
            var id = Guid.NewGuid();
            return new UserInfo
            {
                Id = id,
                IsSavedToCloud = false,
            };
        }
    }
}
