
using System.Diagnostics;
using Windows.ApplicationModel.Contacts;
using Windows.System.UserProfile;

namespace Microsoft.Toolkit.Graph.Controls.Extensions
{
    public static class GraphExtensions
    {
        public static Contact ToWindowsContact(this Microsoft.Graph.Person person)
        {
            var contact = new Contact()
            {
                RemoteId = person.Id,
                DisplayNameOverride = person.DisplayName,
                // Note: Trying to set individual name portions causes UnhandledException in contract call
            };

            foreach (var phoneInfo in person.Phones)
            {
                contact.Phones.Add(new ContactPhone()
                {
                    Kind = phoneInfo.Type.ToPhoneKind(),
                    Number = phoneInfo.Number,
                });
            }

            foreach (var emailInfo in person.EmailAddresses)
            {
                contact.Emails.Add(new ContactEmail()
                {
                    Address = emailInfo.Address,
                });
            }

            contact.JobInfo.Add(new ContactJobInfo()
            {
                Title = person.Title ?? string.Empty,
                CompanyName = person.CompanyName ?? string.Empty,
                CompanyYomiName = person.YomiCompany ?? string.Empty,
                Department = person.Department ?? string.Empty,
                Office = person.OfficeLocation ?? string.Empty,
            });

            return contact;
        }

        public static ContactPhoneKind ToPhoneKind(this Microsoft.Graph.PhoneType? phoneType)
        {
            if (phoneType is null)
            {
                return ContactPhoneKind.Other;
            }

            switch (phoneType.Value)
            {
                case Microsoft.Graph.PhoneType.Home:
                    return ContactPhoneKind.Home;
                case Microsoft.Graph.PhoneType.Business:
                    return ContactPhoneKind.Company;
                case Microsoft.Graph.PhoneType.Mobile:
                    return ContactPhoneKind.Mobile;
                default:
                case Microsoft.Graph.PhoneType.Other:
                case Microsoft.Graph.PhoneType.OtherFax:
                    return ContactPhoneKind.Other;
                case Microsoft.Graph.PhoneType.Assistant:
                    return ContactPhoneKind.Assistant;
                case Microsoft.Graph.PhoneType.HomeFax:
                    return ContactPhoneKind.HomeFax;
                case Microsoft.Graph.PhoneType.BusinessFax:
                    return ContactPhoneKind.BusinessFax;
                case Microsoft.Graph.PhoneType.Pager:
                    return ContactPhoneKind.Pager;
                case Microsoft.Graph.PhoneType.Radio:
                    return ContactPhoneKind.Radio;
            }
        }
    }
}
