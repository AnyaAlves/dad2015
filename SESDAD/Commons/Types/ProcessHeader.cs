using System;

namespace SESDAD.Commons {

    [Serializable]
    public class ProcessHeader : IEquatable<ProcessHeader> {
        private String processName,
                       siteName,
                       processURL;
        private ProcessType processType;

        public ProcessHeader(
            String newProcessName,
            ProcessType newProcessType,
            String newSiteName,
            String newProcessURL) {
            processName = newProcessName;
            processType = newProcessType;
            siteName = newSiteName;
            processURL = newProcessURL;
        }

        public String ProcessName {
            get { return processName; }
        }
        public ProcessType ProcessType {
            get { return processType; }
        }
        public String SiteName {
            get { return siteName; }
        }
        public String ProcessURL {
            get { return processURL; }
        }

        public override bool Equals(object obj) {
            return Equals(obj as ProcessHeader);
        }
        public bool Equals(ProcessHeader obj) {
            return obj != null && processURL.Equals(obj.processURL);
        }

        public override int GetHashCode() {
            return processURL.GetHashCode();
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
