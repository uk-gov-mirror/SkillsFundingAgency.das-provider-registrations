using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.ProviderRegistrations.Data;
using SFA.DAS.ProviderRegistrations.Models;

namespace SFA.DAS.ProviderRegistrations.Application.Commands.AddInvitationCommand
{
    public class AddInvitationCommandHandler : IRequestHandler<AddInvitationCommand, string>
    {
        private readonly Lazy<ProviderRegistrationsDbContext> _db;

        public AddInvitationCommandHandler(Lazy<ProviderRegistrationsDbContext> db)
        {
            _db = db;
        }

        public async Task<string> Handle(AddInvitationCommand request, CancellationToken cancellationToken)
        {
            var reference = Guid.NewGuid();

            var invitation = new Invitation(
                reference,
                request.Ukprn,
                request.UserRef,
                request.EmployerOrganisation,
                request.EmployerFirstName,
                request.EmployerLastName,
                request.EmployerEmail,
                0,
                DateTime.Now,
                DateTime.Now);

            _db.Value.Invitations.Add(invitation);
            await _db.Value.SaveChangesAsync(cancellationToken);

            return reference.ToString();
        }
    }
}