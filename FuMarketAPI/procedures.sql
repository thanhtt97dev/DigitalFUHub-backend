USE [DBTest]
GO;

-- Reports
CREATE PROCEDURE dbo.getUserReport
	@id INT
AS
BEGIN
	SELECT * FROM [User] WHERE [User].UserId = @id;
END;
GO;

-- Danh sách những người gửi đang tham gia vào cuộc trò chuyện
CREATE PROCEDURE dbo.GetSenderConversation
	@userId int
AS
BEGIN
	
	SELECT DISTINCT
	u.*,
	m.ConversationId
	FROM [User] as u
	INNER JOIN [Message] as m ON u.UserId = m.UserId
	WHERE m.ConversationId IN (
	SELECT 
		ConversationId
	FROM UserConversation WHERE UserId = 4)
END;
GO;

-- EXEC
/*
	EXEC dbo.GetSenderConversation 4;
	GO;
*/

-- DROP
/*
	DROP PROCEDURE dbo.GetSenderConversation
*/


-- Tạo cuộc trò chuyện/gửi tin nhắn
CREATE PROCEDURE dbo.SendChatMessage
	@conversationId INT, @senderId INT, @recipientId INT, @content TEXT, @dateCreate DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
			DECLARE @conversationIdCur INT
			IF (@conversationId = 0 )
				BEGIN
					BEGIN TRANSACTION
					INSERT INTO [Conversation] (DateCreate, isActivate)
					VALUES (GETDATE(), 1)

					SELECT @conversationIdCur = IDENT_CURRENT(N'dbo.Conversation')

					INSERT INTO [UserConversation] (UserId, ConversationId)
					VALUES (@senderId, @conversationIdCur),
							(@recipientId, @conversationIdCur)

					INSERT INTO [Message] (UserId, ConversationId, [Content], MessageType, DateCreate, isDelete)
					VALUES (@senderId, @conversationIdCur, @content, '1', @dateCreate, '0')
				END;
			ELSE 
				BEGIN
					BEGIN TRANSACTION
					INSERT INTO [Message] (UserId, ConversationId, [Content], MessageType, DateCreate, isDelete)
					VALUES (@senderId, @conversationId, @content, '1', @dateCreate, '0');
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
	EXEC dbo.SendChatMessage 0, 9, 4, 'Xin chao Hieu, minh la Hoang', '2023-09-16';
	EXEC dbo.SendChatMessage 0, 0, 4, 'Xin chao Hieu, minh la Hoang', '2023-09-16';
	EXEC dbo.SendChatMessage 4, 4, 9, 'Ok. Chao Hoang nhe', '2023-09-16';
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