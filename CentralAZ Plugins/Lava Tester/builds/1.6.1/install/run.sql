
        -----------------------------------------------------------------------
        -- AddPage
        -----------------------------------------------------------------------
		        DECLARE @PageId int
                DECLARE @ParentPageId int = ( SELECT [Id] FROM [Page] WHERE [Guid] = '7F1F4130-CB98-473B-9DE1-7A886D2283ED' )
                DECLARE @LayoutId int = ( SELECT [Id] FROM [Layout] WHERE [Guid] = 'D65F783D-87A9-4CC9-8110-E83466A0EADB' )
                DECLARE @Order int = ( SELECT [order] + 1 FROM [Page] WHERE [Guid] = NULL )

                IF @Order IS NULL
                BEGIN
                    SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;
                END
                ELSE
                BEGIN
                    UPDATE [Page] SET [Order] = [Order] + 1 WHERE [ParentPageId] = @ParentPageId AND [Order] >= @Order
                END

				SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6A588BAA-9A93-4D4F-9700-2B6FF638296B')
                IF @PageId IS NULL
				BEGIN
					INSERT INTO [Page] (
						[InternalName],[PageTitle],[BrowserTitle],[IsSystem],[ParentPageId],[LayoutId],
						[RequiresEncryption],[EnableViewState],
						[PageDisplayTitle],[PageDisplayBreadCrumb],[PageDisplayIcon],[PageDisplayDescription],
						[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
						[BreadCrumbDisplayName],[BreadCrumbDisplayIcon],
						[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
						[IconCssClass],[Guid], [SiteId])
					VALUES(
						'Lava Tester','Lava Tester','Lava Tester',0,@ParentPageId,@LayoutId,
						0,1,
						1,1,1,1,
						0,0,1,0,
						1,0,
						@Order,0,'',1,
						'fa fa-bug','6A588BAA-9A93-4D4F-9700-2B6FF638296B', 1)
				END
				ELSE
				BEGIN
					UPDATE [Page]
					SET [SiteId] = 1
					WHERE [Id] = @PageId
				END

        -----------------------------------------------------------------------
        -- UpdateBlockType
        -----------------------------------------------------------------------
                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [BlockType] WHERE [Path] = '~/Plugins/com_centralaz/Utility/LavaTester.ascx')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [BlockType] (
                        [IsSystem],[Path],[Category],[Name],[Description],
                        [Guid])
                    VALUES(
                        1,'~/Plugins/com_centralaz/Utility/LavaTester.ascx','com_centralaz > Utility','Lava Tester','Allows you to pick a person, group, workflow instance, or registration entity and test your lava.',
                        'E32C203C-8091-45C1-B7D2-9950A6FB480B')
                END
                ELSE
                BEGIN
                    UPDATE [BlockType] SET
                        [IsSystem] = 0,
                        [Category] = 'com_centralaz > Utility',
                        [Name] = 'Lava Tester',
                        [Description] = 'Allows you to pick a person, group, workflow instance, or registration entity and test your lava.',
                        [Guid] = 'E32C203C-8091-45C1-B7D2-9950A6FB480B'
                    WHERE [Path] = '~/Plugins/com_centralaz/Utility/LavaTester.ascx'
                END


        -----------------------------------------------------------------------
        -- AddBlock
        -----------------------------------------------------------------------
                SET @PageId = null

                SET @LayoutId = null

                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6A588BAA-9A93-4D4F-9700-2B6FF638296B')

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'E32C203C-8091-45C1-B7D2-9950A6FB480B')
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockId int
				SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = 'E1A62FA0-AC3B-4EF5-B7B1-B20F1D5AAD8C')
                IF @BlockId IS NULL
				BEGIN
					INSERT INTO [Block] (
						[IsSystem],[PageId],[LayoutId],[BlockTypeId],[Zone],
						[Order],[Name],[PreHtml],[PostHtml],[OutputCacheDuration],
						[Guid])
					VALUES(
						0,@PageId,@LayoutId,@BlockTypeId,'Main',
						0,'Lava Tester','','',0,
						'E1A62FA0-AC3B-4EF5-B7B1-B20F1D5AAD8C')
					SET @BlockId = SCOPE_IDENTITY()
				END