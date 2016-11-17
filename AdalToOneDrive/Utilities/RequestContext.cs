namespace BMO.SPO.Audit.Activity.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using BMO.SPO.Audit.Activity.Logger.Utilities;

    public class RequestContext
    {
        public string TenantId;
        public string ContentId;
        public string[] ContentTypes;
        public string ContentUrl;
        public string StartTime;
        public string EndTime;
        public string Token;
        public DateTime LogDay;
        public int PartitionIntervalMinutes;

        public RequestContext(Dictionary<string, string> parameters)
        {
            try
            {
                // default values from configuration
                TenantId = ConfigurationManager.AppSettings["TenantId"];
                ContentTypes = ConfigurationManager.AppSettings["ContentType"].Split(',');
                ContentUrl = ConfigurationManager.AppSettings["ActivityApiBaseUrl"]; // url of the tenant
                PartitionIntervalMinutes = Int32.Parse(ConfigurationManager.AppSettings["AvailableContentPartitionIntervalMinutes"]);
                if (1440  % PartitionIntervalMinutes  != 0)
                    throw new LoggerException(String.Format("PartitionIntervalMinutes of {0} needs to divisible by 1440  minutes", PartitionIntervalMinutes));
                // parsed parameters
                TenantId = parameters.ContainsKey("tenantid") ? parameters["tenantid"] : TenantId;
                ContentTypes = parameters.ContainsKey("contenttype") ? parameters["contenttype"].Split(',') : ContentTypes;
                ContentUrl = parameters.ContainsKey("contenturl") ? parameters["contenturl"] : "";
                StartTime = parameters.ContainsKey("starttime") ? parameters["starttime"] : "";
                EndTime = parameters.ContainsKey("endtime") ? parameters["endtime"] : "";
                LogDay = parameters.ContainsKey("logDay") ? DateTime.Parse(parameters["logDay"]) : DateTime.Now;
                
            }
            catch (Exception ex)
            {
                throw new LoggerException(String.Format("RequestContext exception with reading from app settings or access to parameters dictionary. {0}", ex.Message), ex);
            }
            
        }
    }
}
