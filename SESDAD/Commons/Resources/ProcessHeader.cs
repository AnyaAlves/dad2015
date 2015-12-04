using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SESDAD.Commons {

    [Serializable]
    public class ProcessHeader : IEquatable<ProcessHeader>, ISerializable {
        public String ProcessName { get; private set; }
        public ProcessType ProcessType { get; private set; }
        public String SiteName { get; private set; }
        public String ProcessURL { get; private set; }

        public ProcessHeader() {}

        public ProcessHeader(String processName, ProcessType processType, String siteName, String processURL) {
            ProcessName = processName;
            ProcessType = processType;
            SiteName = siteName;
            ProcessURL = processURL;
        }

        public ProcessHeader(SerializationInfo info, StreamingContext context) {
            ProcessName = info.GetString("processName");
            ProcessType = (ProcessType)info.GetValue("processType", typeof(ProcessType));
            SiteName = info.GetString("sitename");
            ProcessURL = info.GetString("processURL");
        }

        public override bool Equals(object obj) {
            return Equals(obj as ProcessHeader);
        }
        public bool Equals(ProcessHeader obj) {
            return obj != null && ProcessURL.Equals(obj.ProcessURL);
        }

        public override int GetHashCode() {
            return ProcessURL.GetHashCode();
        }

        public static String operator+(ProcessHeader left, ProcessHeader right) {
            //return left.ProcessURL + right.ProcessURL;
            return left.ProcessName + right.ProcessName;
        }

        public ProcessHeader Clone() {
            ProcessHeader other = (ProcessHeader)this.MemberwiseClone();
            other.ProcessName = String.Copy(ProcessName);
            other.SiteName = String.Copy(SiteName);
            other.ProcessURL = String.Copy(ProcessURL);
            return other;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("processName", ProcessName, typeof(String));
            info.AddValue("processType", ProcessType, typeof(ProcessType));
            info.AddValue("sitename", SiteName, typeof(String));
            info.AddValue("processURL", ProcessURL, typeof(String));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }

        public override String ToString() {
            String nl = Environment.NewLine;
            return
                " Site Name:    " + SiteName + nl +
                " Process Name: " + ProcessName + nl +
                " Process URL:  " + ProcessURL + nl;
        }
    }
}
