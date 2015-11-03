using System;

namespace SESDAD.CommonTypes {

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

        public override String ToString() {
            return
                "* Site Name:    " + SiteName + Environment.NewLine +
                "* Process Name: " + ProcessName + Environment.NewLine +
                "* Process URL:  " + ProcessURL + Environment.NewLine;
        }
    }
}
