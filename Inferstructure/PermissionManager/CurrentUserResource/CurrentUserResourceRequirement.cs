using Dotnet.fs.Application.Contracts.Persistence;
using Dotnet.fs.Domain.Entities.Profile;
using Dotnet.fs.Domain.Entities.Profile.Real;
using Dotnet.fs.Domain.Entities.Requests;
using Dotnet.fs.Domain.Entities.Requests.FinancialInstitutionApproval;
using Dotnet.fs.SharedKernel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq.Expressions;

namespace Dotnet.fs.Infrastructure.Identity.Identity.PermissionManager.CurrentUserResource;

public class CurrentUserResourceRequirement : IAuthorizationRequirement
{
}

public class CurrentUserResourceHandler : AuthorizationHandler<CurrentUserResourceRequirement>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public CurrentUserResourceHandler(IHttpContextAccessor contextAccessor, IUnitOfWork unitOfWork)
    {
        _contextAccessor = contextAccessor;
        _unitOfWork = unitOfWork;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CurrentUserResourceRequirement requirement)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;

        RouteData routeData = httpContext?.GetRouteData();
        if (routeData is null)
        {
            context.Fail();
            return;
        }

        if (!httpContext.User.Identity.IsAuthenticated)
        {
            context.Fail();
            return;
        }

        string controller = routeData.Values["controller"].ToString();
        string action = routeData.Values["action"].ToString();
        string idString = routeData.Values["id"].ToString();
        int userId = int.Parse(httpContext.User.Identity.GetUserId());

        if (int.TryParse(idString, out int id))
        {
            if (controller.Contains("FundApproval", StringComparison.InvariantCultureIgnoreCase) ||
                controller.Contains("BrokerageApproval", StringComparison.InvariantCultureIgnoreCase) ||
                controller.Contains("FinancialinstitutionApproval", StringComparison.InvariantCultureIgnoreCase) ||
                controller.Contains("InvestmentFundApproval", StringComparison.InvariantCultureIgnoreCase) ||
                controller.Contains("EstablishmentRequest", StringComparison.InvariantCultureIgnoreCase))
            {
                Request fund = await _unitOfWork.GetRepository<Request>().FindOne(fund => fund.Id == id);
                if (fund.DemandantUserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("education", StringComparison.InvariantCultureIgnoreCase))
            {
                Education education = await _unitOfWork.GetRepository<Education>().FindOne(x => x.Id == id,
                    new List<Expression<Func<Education, object>>> { x => x.Profile });
                if (education?.Profile.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("occupation", StringComparison.InvariantCultureIgnoreCase))
            {
                Occupation occupation = await _unitOfWork.GetRepository<Occupation>()
                    .FindOne(x => x.Id == id,
                        new List<Expression<Func<Occupation, object>>> { x => x.Profile });
                ;
                if (occupation?.Profile.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("relation", StringComparison.InvariantCultureIgnoreCase))
            {
                RelatedPerson relatedPerson = await _unitOfWork.GetRepository<RelatedPerson>().FindOne(x => x.Id == id,
                    new List<Expression<Func<RelatedPerson, object>>> { x => x.Profile });
                if (relatedPerson?.Profile.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("financial", StringComparison.InvariantCultureIgnoreCase))
            {
                FinancialStatement financialStatement = await _unitOfWork.FinancialStatementRepository
                    .GetFinancialStatementByIdAsync(id);
                if (financialStatement?.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("board", StringComparison.InvariantCultureIgnoreCase))
            {
                BoardMember boardMember = await _unitOfWork.BoardRepository
                    .GetBoardMemberByIdAsync(id);
                if (boardMember?.LegalPerson.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("shareholder", StringComparison.InvariantCultureIgnoreCase))
            {
                Shareholder shareholder = await _unitOfWork.ShareholderRepository.GetShareholderByIdAsync(id);
                if (shareholder?.LegalPerson.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("ProfessionalDegree", StringComparison.InvariantCultureIgnoreCase))
            {
                ProfessionalDegree pd = await _unitOfWork.GetRepository<ProfessionalDegree>()
                    .FindOne(x => x.Id == id,
                        new List<Expression<Func<ProfessionalDegree, object>>> { x => x.Profile });
                if (pd?.Profile.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("FurtherInformationAnswer", StringComparison.InvariantCultureIgnoreCase))
            {
                FurtherInformationAnswer pd = await _unitOfWork.GetRepository<FurtherInformationAnswer>()
                    .FindOne(x => x.Id == id,
                        new List<Expression<Func<FurtherInformationAnswer, object>>> { x => x.RealProfile });
                if (pd?.RealProfile.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }

            else if (action.Contains("newspaper", StringComparison.InvariantCultureIgnoreCase))
            {
                OfficialNewspaper officialNewspaper = _unitOfWork.GetRepository<OfficialNewspaper>()
                    .Query(c => c.Id == id, new List<string> { nameof(OfficialNewspaper.LegalPerson) })
                    .GetAwaiter().GetResult().FirstOrDefault();
                if (officialNewspaper?.LegalPerson.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("LegalFurther", StringComparison.InvariantCultureIgnoreCase))
            {
                LegalPerson legalPerson = _unitOfWork.GetRepository<LegalPerson>()
                    .FindFirst(c => c.UserId == userId, new List<string>()).GetAwaiter().GetResult();
                LegalFurtherInformationAnswer legalFurtherInformationAnswer = _unitOfWork
                    .GetRepository<LegalFurtherInformationAnswer>()
                    .FindFirst(c =>
                            c.LegalPersonId == legalPerson.Id &&
                            c.QuestionId == id &&
                            c.DeletedDate == null,
                        new List<string> { nameof(LegalFurtherInformationAnswer.LegalPerson) })
                    .GetAwaiter().GetResult();
                if (legalFurtherInformationAnswer?.LegalPerson.UserId == userId)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else if (action.Contains("CalculateScore", StringComparison.InvariantCultureIgnoreCase) ||
                     action.Contains("PaymentOfFees", StringComparison.InvariantCultureIgnoreCase))
            {
                var financial = _unitOfWork.GetRepository<FinancialInstitutionApprovalRequest>()
                    .FindOne(c => c.Request.DemandantUserId == userId && c.RequestId == id,
                        new List<Expression<Func<FinancialInstitutionApprovalRequest, object>>>()).GetAwaiter()
                    .GetResult();
                if (financial is not null)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, "No Id Found"));
        }
    }
}