 
 --This script assigns each neighborhood group the proper campus
 
 
 --Get Parent Group Ids 
 Declare @AhwatukeeParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = '26EA17B8-23CC-4B19-81A7-C3399BB6D84D')
 Declare @GlendaleParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = '8349D1D4-1CD7-49EE-8ED5-D74A339B8A82')
 Declare @QueenCreekParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = '97BEFE4C-6ECF-4DAF-AE8E-36D2D45C639C')
 Declare @GilbertEastParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = '36ED7BDB-9F34-42F4-BACA-961CBDA4E13D')
 Declare @GilbertWestParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = '6C4ACB1F-60A8-44E5-B272-87827C534B97')
 Declare @MesaEastParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = 'F7970E28-7A35-4708-97B1-811F8718A1AD')
 Declare @MesaWestParentGroupId int = (Select Top 1 [Id] From [Group] Where [Guid] = 'AD55187B-43C2-414C-812F-87C75C2D8467')

 -- Get Campus Ids
 Declare @AhwatukeeCampusId int = (Select Top 1 [Id] From [Campus] Where [Guid] = 'B450CE45-37E8-4295-9BF9-11F907714FFA')
 Declare @GlendaleCampusId int = (Select Top 1 [Id] From [Campus] Where [Guid] = '6A23A30B-A4DA-4240-9449-B5B227233B30')
 Declare @QueenCreekCampusId int = (Select Top 1 [Id] From [Campus] Where [Guid] = '9329C09C-B41A-408F-9DEF-CE5A743D6CFC')
 Declare @GilbertCampusId int = (Select Top 1 [Id] From [Campus] Where [Guid] = '0BF5BD9B-2D8E-4215-999C-EAA46165BD9F')
 Declare @MesaCampusId int = (Select Top 1 [Id] From [Campus] Where [Guid] = '76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8')

  /*  Ahwatukee  */
  Update [Group]
  Set CampusId = @AhwatukeeCampusId
  Where ParentGroupId = @AhwatukeeParentGroupId

  /*  Glendale  */
  Update [Group]
  Set CampusId = @GlendaleCampusId
  Where ParentGroupId = @GlendaleParentGroupId

  /*  Queen Creek  */
  Update [Group]
  Set CampusId = @QueenCreekCampusId
  Where ParentGroupId = @QueenCreekParentGroupId

  /*  Gilbert  */
  Update [Group]
  Set CampusId = @GilbertCampusId
  Where ParentGroupId = @GilbertEastParentGroupId Or ParentGroupId = @GilbertWestParentGroupId

  /*  Mesa  */
  Update [Group]
  Set CampusId = @MesaCampusId
  Where ParentGroupId = @MesaEastParentGroupId Or ParentGroupId = @MesaWestParentGroupId