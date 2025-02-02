﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Provider.Shared.UI.Attributes;
using SFA.DAS.ProviderRegistrations.Application.Commands.AddInvitationCommand;
using SFA.DAS.ProviderRegistrations.Application.Commands.SendInvitationEmailCommand;
using SFA.DAS.ProviderRegistrations.Application.Queries.GetInvitationQuery;
using SFA.DAS.ProviderRegistrations.Application.Queries.GetProviderByUkprnQuery;
using SFA.DAS.ProviderRegistrations.Configuration;
using SFA.DAS.ProviderRegistrations.Types;
using SFA.DAS.ProviderRegistrations.Web.Authentication;
using SFA.DAS.ProviderRegistrations.Web.Extensions;
using SFA.DAS.ProviderRegistrations.Web.ViewModels;
using SFA.DAS.ProviderUrlHelper.Core;

namespace SFA.DAS.ProviderRegistrations.Web.Controllers
{
    [Route("{providerId}/[controller]/[action]")]
    public class RegistrationController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAuthenticationService _authenticationService;
        private readonly ProviderRegistrationsSettings _configuration;

        public RegistrationController(
            IMediator mediator, 
            IMapper mapper, 
            IAuthenticationService authenticationService,
            ProviderRegistrationsSettings configuration)
        {
            _mediator = mediator;
            _mapper = mapper;
            _authenticationService = authenticationService;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public IActionResult StartAccountSetup()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public IActionResult NewEmployerUser()
        { 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public IActionResult NewEmployerUser(NewEmployerUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("NewEmployerUser", model);
            }

            return View("ReviewDetails", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> InviteEmployeruser(NewEmployerUserViewModel model, string command)
        {
            if (command == "Change")
            {
                return View("NewEmployerUser", model);
            }

            if (!ModelState.IsValid)
            {
                return View("ReviewDetails", model);
            }

            var ukprn = _authenticationService.Ukprn.Value;
            var userId = _authenticationService.UserId;
            _authenticationService.TryGetUserClaimValue(ProviderClaims.DisplayName, out string providerUserFullName);

            var provider = await _mediator.Send(new GetProviderByUkprnQuery(ukprn), new CancellationToken());
                        
            var employerOrganisation = model.EmployerOrganisation.Trim();
            var employerFirstName = model.EmployerFirstName.Trim();
            var employerLastName = model.EmployerLastName.Trim();
            var employerEmail = model.EmployerEmailAddress.Trim().ToLower();
            var employerFullName = string.Concat(employerFirstName, " ", employerLastName);
            
            var correlationId = await _mediator.Send(new AddInvitationCommand(ukprn, userId, provider.ProviderName, providerUserFullName, employerOrganisation, employerFirstName, employerLastName, employerEmail));
            await _mediator.Send(new SendInvitationEmailCommand(ukprn,provider.ProviderName, providerUserFullName, employerOrganisation, employerFullName, employerEmail, correlationId));

            return View("InviteConfirmation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public IActionResult InviteConfirmation(string action)
        {
            switch (action)
            {
                case "Invite": return Redirect(@Url.ProviderAction("StartAccountSetup"));
                case "View": return Redirect(@Url.ProviderAction("InvitedEmployers"));
                case "Homepage": return Redirect(@Url.ProviderApprenticeshipServiceLink(""));
                default:
                {
                    ViewBag.InValid = true;
                    return View();
                }
            }
        }

        [HttpGet]
        [Authorize(Policy = nameof(PolicyNames.HasViewerOrAbovePermission))]
        public async Task<IActionResult> InvitedEmployers(string sortColumn, string sortDirection)
        {
            sortColumn = Enum.GetNames(typeof(InvitationSortColumn)).SingleOrDefault(e => e == sortColumn);

            if (string.IsNullOrWhiteSpace(sortColumn)) sortColumn = Enum.GetNames(typeof(InvitationSortColumn)).First();
            if (string.IsNullOrWhiteSpace(sortDirection) || (sortDirection != "Asc" && sortDirection != "Desc")) sortDirection = "Asc";

            var results = await _mediator.Send(new GetInvitationQuery(_authenticationService.Ukprn.GetValueOrDefault(0), null, sortColumn, sortDirection));

            var model = _mapper.Map<InvitationsViewModel>(results);
            model.SortColumn = sortColumn;
            model.SortDirection = sortDirection;

            return View(model);
        }

        [HttpGet]
        [HideNavigationBar]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<ActionResult> EmailPreview(string employerName, string employerOrganisation)
        {
            var provider = await _mediator.Send(new GetProviderByUkprnQuery(_authenticationService.Ukprn.GetValueOrDefault(0)));

            return View(new EmailPreviewViewModel
            {
                EmployerName = employerName,
                EmployerOrganisation = employerOrganisation,
                ProviderOrganisation = provider.ProviderName,
                ProviderName = _authenticationService.UserName,
                EmployerAccountsUrl = $"{_configuration.EmployerAccountsBaseUrl}/service/register"
            });
        }
    }
}
