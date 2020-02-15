# Sugang API

Tiny API for `sugang.inha.ac.kr`

The use of this source code to create a course registration macro is prohibited.
The developer assumes no responsibility for any violations of the university rules and the laws.
In addition, the developer is not responsible for any problems resulting from the modification and use of this program.

If you want to create a timetable, see https://github.com/rollrat/InhaTT.

## 1. Sugang Methods

### 1.1. Login

Login method is very simple.
Using the DPI bypass program when using this method can cause critical errors.

``` cs
var session = SugangSession.Create("id", "password");

// Check failing to login.
if (session == SugangSession.ErrorSession)
  throw new Exception("Fail to login!");
```

### 1.2. Time Table Methods

#### 1.2.1. Enumeration current season subjects list

``` cs
public class Subject
{
    public string Hacksu;
    public string Group;
    public string Name;
    public string Class;
    public string Score;
    public string Type;
    public string Time;
    public string Professor;
    public string Department;
    public string Estimation;
    public string Remain;
    public string Bigo;
}

List<Subject> subjects = SugangUtils.LoadCurrentSeasonSubjects();
```

#### 1.2.2. Get subscribed courses - Login required

``` cs
List<Subject> subjects = session.GetSubscribedCourses();
```

### 1.3. Course Application Method

## 2. Mail Method

### 2.1. Query Address by Member Name

``` cs
public class SearchResult
{
    public string Id;
    public string Name;
    public string Email;
    public string Position;
    public string[] DutyName;
    public string NodeType;
    public string[] Departments;
    public string[] DepartmentsIds;
}

var session = MailSession.Create("id", "password");

List<SearchResult> results =  session.QueryAddress("rollrat");
```