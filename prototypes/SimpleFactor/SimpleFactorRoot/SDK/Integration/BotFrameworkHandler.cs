using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace SimpleFactorRoot.SDK
{
    public class BotFrameworkHandler
    {
        public virtual Task OnGetActivityMembersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnSendConversationHistoryAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnReplyToActivityAsync(string conversationId, string activityId, Activity replyToActivity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnUpdateActivityAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnDeleteActivityAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnSendToConversationAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnDeleteConversationMemberAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnUploadAttachmentAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnGetConversationMembersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnGetConversationPagedMembersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnGetConversationsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OnCreateConversationAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
