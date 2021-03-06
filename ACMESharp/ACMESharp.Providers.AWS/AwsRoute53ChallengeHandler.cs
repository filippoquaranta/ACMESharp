using System;
using System.Text.RegularExpressions;
using ACMESharp.ACME;
using System.IO;

namespace ACMESharp.Providers.AWS
{
    public class AwsRoute53ChallengeHandler : IChallengeHandler
    {
        #region -- Properties --

        public string HostedZoneId
        { get; set; }

        public string ResourceRecordType
        { get; set; } = "TXT";

        public int ResourceRecordTtl
        { get; set; } = 300;

        public AwsCommonParams CommonParams
        { get; set; } = new AwsCommonParams();

        public bool IsDisposed
        { get; private set; }

        #endregion -- Properties --

        #region -- Methods --

        public void Handle(ChallengeHandlingContext ctx)
        {
            AssertNotDisposed();
            EditDns((DnsChallenge) ctx.Challenge, false, ctx.Out);
        }

        public void CleanUp(ChallengeHandlingContext ctx)
        {
            AssertNotDisposed();
            EditDns((DnsChallenge)ctx.Challenge, true, ctx.Out);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed)
                throw new InvalidOperationException("AWS Challenge Handler is disposed");
        }

        private void EditDns(DnsChallenge dnsChallenge, bool delete, TextWriter msg)
        {
            var dnsName = dnsChallenge.RecordName;
            var dnsValue = Regex.Replace(dnsChallenge.RecordValue, "\\s", "");
            var dnsValues = Regex.Replace(dnsValue, "(.{100,100})", "$1\n").Split('\n');

            var r53 = new Route53Helper
            {
                HostedZoneId = HostedZoneId,
                ResourceRecordTtl = ResourceRecordTtl,
                CommonParams = CommonParams,
            };

            if (ResourceRecordType == "TXT")
                r53.EditTxtRecord(dnsName, dnsValues, delete);
            else
                throw new NotImplementedException($"RR type of [{ResourceRecordType}] not implemented");
        }

        #endregion -- Methods --
    }
}