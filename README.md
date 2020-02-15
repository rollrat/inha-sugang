# Sugang API

Tiny API for `sugang.inha.ac.kr`

The use of this source code to create a course registration macro is prohibited.
The developer assumes no responsibility for any violations of the university rules and the laws.
In addition, the developer is not responsible for any problems resulting from the modification and use of this program.

If you want to create a timetable, see https://github.com/rollrat/InhaTT.

## How to use

### 1. Login

Login method is very simple.
Using the DPI bypass program when using this method can cause critical errors.

``` cs
var session = SugangSession.Create("id", "password");

// Check failing to login.
if (session == SugangSession.ErrorSession)
  throw new Exception("Fail to login!");
```

### 2. Time Table Methods

#### 2.1. Enumeration current season subjects list

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

#### 2.2. Get subscribed courses - Login required

``` cs
List<Subject> subjects = session.GetSubscribedCourses();
```

### 3. Course Application Method