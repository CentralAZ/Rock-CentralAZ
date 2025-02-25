        -----------------------------------------------------------------------
        -- DeleteBlock
        -----------------------------------------------------------------------
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = 'E1A62FA0-AC3B-4EF5-B7B1-B20F1D5AAD8C')
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
                DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockId
                DELETE [Block] WHERE [Guid] = 'E1A62FA0-AC3B-4EF5-B7B1-B20F1D5AAD8C'

        -----------------------------------------------------------------------
        -- DeleteBlock
        -----------------------------------------------------------------------
        DELETE [BlockType] WHERE [Guid] = 'E32C203C-8091-45C1-B7D2-9950A6FB480B'

        -----------------------------------------------------------------------
        -- DeletePage
        -----------------------------------------------------------------------
                DELETE PV
                FROM [PageView] PV
                INNER JOIN [Page] P ON P.[Id] = PV.[PageId] AND P.[Guid] = '6A588BAA-9A93-4D4F-9700-2B6FF638296B'

                DELETE [Page] WHERE [Guid] = '6A588BAA-9A93-4D4F-9700-2B6FF638296B'
