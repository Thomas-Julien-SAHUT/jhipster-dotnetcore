using System.Threading.Tasks;
using JhipsterSampleApplication.Domain;

namespace JhipsterSampleApplication.Domain.Services.Interfaces {
    public interface IMailService {
        Task SendPasswordResetMail(User user);
        Task SendActivationEmail(User user);
        Task SendCreationEmail(User user);
    }
}
