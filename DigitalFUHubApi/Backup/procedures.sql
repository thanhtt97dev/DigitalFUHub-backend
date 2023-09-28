/*
	USE database và execute để tạo procedures
*/

USE [DBTest]
GO;

-- Lấy danh sách những người gửi đang tham gia vào cuộc trò chuyện
    CREATE PROCEDURE dbo.GetSenderConversation
        @userId INT
    AS
    BEGIN
        SELECT 
        u.*, 
        r.ConversationId 
        FROM [User] as u,
        (SELECT UserId, ConversationId FROM UserConversation as u WHERE ConversationId IN (
        SELECT ConversationId FROM UserConversation WHERE UserId = @userId)) as r
        WHERE u.UserId = r.userId
        AND u.UserId != @userId
		ORDER BY r.ConversationId DESC
    END;
GO;

-- EXEC
/*
	EXEC dbo.GetSenderConversation 1;
	GO;
*/

-- DROP
/*
	DROP PROCEDURE dbo.GetSenderConversation
*/

-- Lấy danh sách tin nhắn theo cuộc trò chuyện
/*
	SELECT 
	*
	FROM [Message]
	WHERE ConversationId = 2
	ORDER BY DateCreate ASC;
	GO;
*/


-- Tạo cuộc trò chuyện/gửi tin nhắn
CREATE PROCEDURE dbo.SendChatMessage
	@conversationId INT, @senderId INT, @recipientId INT, @content NVARCHAR(MAX), @dateCreate DATETIME, @messageType VARCHAR(1)
AS
BEGIN
	-- SET NOCOUNT ON;
	BEGIN TRY
			DECLARE @conversationIdCur INT
			IF (@conversationId = 0 )
				BEGIN
					BEGIN TRANSACTION
					INSERT INTO [Conversations] (DateCreate, isActivate)
					VALUES (GETDATE(), 1)

					SELECT @conversationIdCur = IDENT_CURRENT(N'dbo.Conversations')

					INSERT INTO [UserConversation] (UserId, ConversationId)
					VALUES (@senderId, @conversationIdCur),
							(@recipientId, @conversationIdCur)

					INSERT INTO [Messages] (UserId, ConversationId, [Content], MessageType, DateCreate, isDelete)
					VALUES (@senderId, @conversationIdCur, @content, @messageType, @dateCreate, '0')
				END;
			ELSE 
				BEGIN
					BEGIN TRANSACTION
					INSERT INTO [Messages] (UserId, ConversationId, [Content], MessageType, DateCreate, isDelete)
					VALUES (@senderId, @conversationId, @content, @messageType, @dateCreate, '0');
				END
			COMMIT;
	END TRY
	BEGIN CATCH
		DECLARE @error INT, @message VARCHAR(4000)
        SELECT @error = ERROR_NUMBER(),
                 @message = ERROR_MESSAGE()
		RAISERROR ('dbo.SendChatMessage: %d: %s', 16, 1, @error, @message) ;
		ROLLBACK;
	END CATCH
	
END;
GO;



-- EXEC
/*
	EXEC dbo.SendChatMessage 1, 1, 2, 'Test isImage', '2023-09-16', 0;
*/

-- DROP PROCEDURE
/*
	DROP PROCEDURE dbo.SendChatMessage;
*/


-- Report Errors
/*
	CREATE PROCEDURE dbo.UspReportErrorSql
	AS
		SELECT   
			ERROR_NUMBER() AS ErrorNumber  
			,ERROR_SEVERITY() AS ErrorSeverity  
			,ERROR_STATE() AS ErrorState  
			,ERROR_LINE () AS ErrorLine  
			,ERROR_PROCEDURE() AS ErrorProcedure  
			,ERROR_MESSAGE() AS ErrorMessage;  
	GO;
*/

-- DROP 
/*
	DROP PROCEDURE dbo.UspReportErrorSql;
*/