using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CustomBlueprints
{
    public interface ILogger
    {
        void Log(string text);
        void Error(string text);
        void Exception(Exception ex);
    }
    public class UMMLogger : ILogger
    {
        UnityModManager.ModEntry.ModLogger logger;
        public UMMLogger(UnityModManager.ModEntry.ModLogger logger)
        {
            this.logger = logger;
        }

        public void Error(string text)
        {
            logger.Error(text);
        }

        public void Exception(Exception ex)
        {
            logger.Error(ex.ToString());
        }

        public void Log(string text)
        {
            logger.Error(text);
        }
    }
}
