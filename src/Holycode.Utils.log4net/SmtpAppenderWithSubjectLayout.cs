#if !CORECLR

namespace log4net.Appender
{
    using log4net.Core;
    using log4net.Layout;
    using System.Collections.Generic;
    using System.IO;


    public class SmtpAppenderWithSubjectLayout : SmtpAppender
    {
        /// <summary>
        ///     Gets or sets the layout for subject line of the e-mail message.
        /// </summary>
        /// <value>
        ///     The layout for subject line of the e-mail message.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The layout for subject line of the e-mail message.
        ///     </para>
        /// </remarks>
        public PatternLayout SubjectLayout { get; set; }


        public SmtpAppenderWithSubjectLayout() : base()
        {

        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            PrepareSubject(events);

            base.SendBuffer(events);
        }

        protected virtual void PrepareSubject(IEnumerable<LoggingEvent> events)
        {
            var subjects = new List<string>();

            foreach (LoggingEvent @event in events)
            {
                if (Evaluator == null || Evaluator.IsTriggeringEvent(@event))
                {
                    using (var sw = new StringWriter())
                    {
                        SubjectLayout.Format(sw, @event);
                        subjects.Add(sw.ToString());
                    }
                }
            }

            Subject = string.Join(", ", subjects.ToArray());
        }
    }
}
#endif