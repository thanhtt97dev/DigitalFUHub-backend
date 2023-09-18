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
	@userId INT, @page INT, @limit INT
AS
BEGIN
	
	SELECT DISTINCT
	u.*,
	m.ConversationId
	FROM [User] as u
	INNER JOIN [Messages] as m ON u.UserId = m.UserId
	WHERE m.ConversationId IN (
	SELECT 
		ConversationId
	FROM [UserConversations] WHERE UserId = @userId)
	ORDER BY m.ConversationId DESC
	OFFSET (@page - 1) * @limit ROWS FETCH NEXT @limit ROWS ONLY;
END;

GO;

-- EXEC
/*
	EXEC dbo.GetSenderConversation 11, 1, 2;
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
					INSERT INTO [Conversations] (DateCreate, isActivate)
					VALUES (GETDATE(), 1)

					SELECT @conversationIdCur = IDENT_CURRENT(N'dbo.Conversations')

					INSERT INTO [UserConversations] (UserId, ConversationId)
					VALUES (@senderId, @conversationIdCur),
							(@recipientId, @conversationIdCur)

					INSERT INTO [Messages] (UserId, ConversationId, [Content], MessageType, DateCreate, isDelete)
					VALUES (@senderId, @conversationIdCur, @content, '1', @dateCreate, '0')
				END;
			ELSE 
				BEGIN
					BEGIN TRANSACTION
					INSERT INTO [Messages] (UserId, ConversationId, [Content], MessageType, DateCreate, isDelete)
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
	EXEC dbo.SendChatMessage 0, 13, 11, 'Xin chao Hieu, minh la Tien', '2023-09-16';
	EXEC dbo.SendChatMessage 0, 15, 11, 'Xin chao Hieu, minh la HieuLD', '2023-09-16';
	EXEC dbo.SendChatMessage 0, 18, 11, 'Xin chao Hieu, minh la Phuc Du', '2023-09-16';

	EXEC dbo.SendChatMessage 0, 0, 4, 'Xin chao Hieu, minh la Hoang', '2023-09-16';
	EXEC dbo.SendChatMessage 4, 4, 9, 'Ok. Chao Hoang nhe', '2023-09-16';
*/

-- DROP PROCEDURE
/*
	DROP PROCEDURE dbo.SendChatMessage;
*/

select * from [User]
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