using System;

namespace SESDAD.CommonTypes {

    [Serializable]
    public class ProcessHeader {

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
            ProcessHeader processHeader = (ProcessHeader)obj;
            return processName.Equals(processHeader.processName) &&
                processType.Equals(processHeader.processType) &&
                siteName.Equals(processHeader.siteName) &&
                processURL.Equals(processHeader.processURL);
        }

        public override int GetHashCode() {
            int hash = 17, prime = 23;
            unchecked {
                if (processName != null) {
                    hash = hash * prime + processName.GetHashCode();
                }
                hash = hash * prime + processType.GetHashCode();
                if (siteName != null) {
                    hash = hash * prime + siteName.GetHashCode();
                }
                if (processURL != null) {
                    hash = hash * prime + processURL.GetHashCode();
                }
            }
            return hash;
        }
    }
}
