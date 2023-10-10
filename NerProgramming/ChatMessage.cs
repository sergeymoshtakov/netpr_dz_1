using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerProgramming
{
    public class ChatMessage
    {
        public String Login { get; set; }
        public String Text { get; set; }
        public DateTime Moment { get; set; }

        public override string ToString()
        {
            return $"{Login}: {Text}";
        }

        public String getDate()
        {
            String answer = "";
            DateTime now = DateTime.Now;
            DateTime thisDate;
            if (Moment != null)
            {
                thisDate = Moment;
            }
            else
            {
                thisDate = DateTime.Now;
            }

            if (now.Date == thisDate.Date)
            {
                answer = "Today " + thisDate.ToString("HH:mm:ss");
            }
            else
            {
                answer = thisDate.ToString("dd.MM.yyyy HH:mm:ss");
            }
            
            return answer;
        }
    }
}
