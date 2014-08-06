
public enum DataBaseTypeEnums
{
   UnitTest = -1,
   SqlServerImplementationI = 1,
   MongoDbImplementationI = 8,
   SqlServerImplementationII = 3,
   MongoDbImplementationII = 4,
   SqlServerImplementationIII = 5,
   MongoDbImplementationIII = 6,
}

public enum TestCaseEnums
{
   UnitTest = -1,
   Initialize = 0,
   TestCase1 = 1,
   TestCase2 = 2,
   TestCase3 = 3,
   TestCase4 = 4,
   TestCase5 = 5,
   TestCase6 = 6,
   TestCase7 = 7,
   TestCase8 = 8,
}

public enum TestScenarioEnums
{
   UnitTest = -1,
   InsertDepartment = 1,
   InsertProject = 2,
   InsertUser = 3,
   UpdateDepartmentNameByKeys = 4,
   UpdateProjectNameByKeys = 5,
   UpdateUserLastNameByFirstName = 6,
   SelectDepartmentByKey = 7,
   SelectDepartmentByRandomName = 8,
   SelectUserByKey = 9,
   SelectUsersByRandomFirstName = 10,
   SelectDepartmentByRandomUserFirstName = 11,
   SelectUsersByRandomProjectKeys = 12,
   SelectAverageAgeByRandomProjectKeysMI = 13,
   SelectAverageAgeByRandomProjectKeysMII = 14,
}

public enum RandomDataTypeEnums
{
   DepartmentId = 1,
   UserId = 2,
   ProjectId = 3,
   UserFirstName = 4,
   DepartmentName = 5,
}