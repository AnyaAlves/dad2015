using System;

namespace SESDAD.Commons {

    [Serializable]
    public class ProcessHeader : IEquatable<ProcessHeader> {
        public String ProcessName { get; private set; }
        public ProcessType ProcessType { get; private set; }
        public String SiteName { get; private set; }
        public String ProcessURL { get; private set; }

        public ProcessHeader(String processName, ProcessType processType, String siteName, String processURL) {
            ProcessName = processName;
            ProcessType = processType;
            SiteName = siteName;
            ProcessURL = processURL;
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

        public override String ToString() {
            String nl = Environment.NewLine;
            return
                " Site Name:    " + SiteName + nl +
                " Process Name: " + ProcessName + nl +
                " Process URL:  " + ProcessURL;
        }
    }
}
