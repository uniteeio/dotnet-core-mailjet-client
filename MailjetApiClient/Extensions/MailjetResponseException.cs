using System.Text;
using Mailjet.Client;

namespace MailjetApiClient.Extensions;

public static class MailjetResponseException
{
    public static string FormatForLogs(this MailjetResponse mailjetResponse)
    {
        var sb = new StringBuilder();
        if (!mailjetResponse.IsSuccessStatusCode)
        {
            sb.AppendLine($"StatusCode: {mailjetResponse.StatusCode}");
            sb.AppendLine(($"ErrorInfo: {mailjetResponse.GetErrorInfo()}"));
            sb.AppendLine((mailjetResponse.GetData().ToString()));
            sb.AppendLine(($"ErrorMessage: {mailjetResponse.GetErrorMessage()}"));
        }
        return sb.ToString();
    }
}