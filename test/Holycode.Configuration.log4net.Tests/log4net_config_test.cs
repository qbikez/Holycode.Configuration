using log4net;
using System;
using Xunit;
using Should;

namespace Tests
{
    public class log4net_config_test
    {
        [Fact]
        public void should_add_global_appender()
        {
            LogManager.ResetConfiguration();
            bool callbackExecuted = false;

            var log = LogManager.GetLogger(typeof(log4net_config_test));
            LogManagerTools.AddCallbackAppender(msg =>
            {
                Console.WriteLine(msg);
                callbackExecuted = true;
            });

            log.Debug("this is a test message");

            callbackExecuted.ShouldBeTrue();
        }

        [Fact]
        public void should_add_appender_to_logger()
        {
            LogManager.ResetConfiguration();
            bool callbackExecuted = false;

            var log = LogManager.GetLogger(typeof(log4net_config_test));

            log.AddCallbackAppender(msg =>
            {
                Console.WriteLine(msg);
                callbackExecuted = true;
            });

            log.Debug("this is a test message");

            callbackExecuted.ShouldBeTrue();
        }

        [Fact]
        public void should_add_appender_to_logger_only()
        {
            LogManager.ResetConfiguration();
            int callbackCount = 0;

            var log = LogManager.GetLogger(typeof(log4net_config_test));
            var rootLog = LogManager.GetLogger("root");

            log.AddCallbackAppender(msg =>
            {
                Console.WriteLine(msg);
                callbackCount++;
            });

            log.Debug("this is a test message");
            rootLog.Debug("this is a root log message");

            callbackCount.ShouldEqual(1);
        }

        [Fact]
        public void root_appender_should_propagate()
        {
            LogManager.ResetConfiguration();
            int callbackCount = 0;

            var rootLog = LogManager.GetLogger("root");
            rootLog.AddCallbackAppender(msg =>
            {
                Console.WriteLine(msg);
                callbackCount++;
            });
            var log = LogManager.GetLogger(typeof(log4net_config_test));

            log.Debug("this is a test message");
            rootLog.Debug("this is a root log message");

            callbackCount.ShouldEqual(2);
        }
    }
}
