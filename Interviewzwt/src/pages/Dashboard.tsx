import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import apiClient, { setToken } from "@/integrations/api/client";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useToast } from "@/hooks/use-toast";
import { Loader2, LogOut, Plus, Briefcase, Code, Server, Rocket, Settings, Palette, Trash2 } from "lucide-react";

interface InterviewSession {
  id: string;
  status: string;
  createdAt: string;
  completedAt?: string;
  resumeScore?: number;
  interviewScore?: number;
  jobRole?: { title: string };
  customJob?: { title: string };
}

interface Profile {
  credits: number;
  full_name: string;
  email: string;
}

interface CustomJob {
  id: string;
  title: string;
  description: string;
  requirements?: string[];
  createdAt?: string;
}

interface JobRole {
  id: string;
  title: string;
  description: string;
  category: string;
  skills: string[];
  icon: string;
}

const Dashboard = () => {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [jobRoles, setJobRoles] = useState<JobRole[]>([]);
  const [customJobs, setCustomJobs] = useState<CustomJob[]>([]);
  const [sessions, setSessions] = useState<InterviewSession[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { toast } = useToast();

  useEffect(() => {
    checkUser();
    fetchJobRoles();
    fetchCustomJobs();
    fetchSessions();
    
    // Re-fetch profile when auth status changes (e.g., after payment capture)
    const handleAuthChange = () => {
      checkUser();
      fetchCustomJobs();
      fetchSessions();
    };
    window.addEventListener('auth-change', handleAuthChange);
    return () => window.removeEventListener('auth-change', handleAuthChange);
  }, []);

  const checkUser = async () => {
    try {
      const { user } = await apiClient.getProfile();
      if (!user) {
        navigate('/auth');
        return;
      }
      setProfile({ credits: user.credits, full_name: user.fullName, email: user.email });
    } catch (error) {
      navigate('/auth');
    } finally {
      setLoading(false);
    }
  };

  const fetchJobRoles = async () => {
    try {
      const roles = await apiClient.listJobRoles();
      setJobRoles(Array.isArray(roles) ? roles : []);
    } catch (error) {
      console.error('Error fetching job roles:', error);
    }
  };

  const fetchCustomJobs = async () => {
    try {
      const jobs = await apiClient.getCustomJobs();
      setCustomJobs(Array.isArray(jobs) ? jobs : []);
    } catch (error) {
      console.error('Error fetching custom jobs:', error);
    }
  };

  const deleteCustomJob = async (id: string) => {
    const confirmed = window.confirm('Are you sure you want to delete this saved job description?');
    if (!confirmed) return;
    try {
      await apiClient.deleteCustomJob(id);
      setCustomJobs((prev) => prev.filter((job) => job.id !== id));
      toast({
        title: 'Saved job deleted',
        description: 'This custom job description has been removed.',
      });
    } catch (error) {
      console.error('Error deleting custom job:', error);
      toast({
        title: 'Unable to delete saved job',
        description: 'Please try again later.',
        variant: 'destructive',
      });
    }
  };

  const normalizeSessions = (data: any): InterviewSession[] => {
    if (Array.isArray(data)) return data;
    if (data && Array.isArray(data.sessions)) return data.sessions;
    if (data && Array.isArray(data.data)) return data.data;
    return [];
  };

  const fetchSessions = async () => {
    try {
      const response = await apiClient.getUserSessions();
      const sessionList = normalizeSessions(response);
      if (!Array.isArray(response)) {
        console.warn('Unexpected session response shape:', response);
      }
      setSessions(sessionList);
    } catch (error) {
      console.error('Error fetching sessions:', error);
      toast({
        title: 'Unable to load past interviews',
        description: 'Please refresh or try again later.',
        variant: 'destructive',
      });
    }
  };

  const handleLogout = async () => {
    setToken(null);
    window.dispatchEvent(new Event('auth-change'));
    navigate('/auth');
  };

  const startInterview = (jobRoleId: string, isCustom = false) => {
    if (profile && profile.credits <= 0) {
      toast({
        title: "No credits remaining",
        description: "Please purchase more credits to continue.",
        variant: "destructive",
      });
      return;
    }
    const suffix = isCustom ? '?custom=true' : '';
    navigate(`/interview/${jobRoleId}${suffix}`);
  };

  const formatDateTime = (dateValue: string) => {
    try {
      return new Date(dateValue).toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    } catch {
      return dateValue;
    }
  };

  const formatDuration = (start: string, end?: string) => {
    if (!end) return 'Still in progress';
    const startDate = new Date(start);
    const endDate = new Date(end);
    const seconds = Math.max(0, Math.floor((endDate.getTime() - startDate.getTime()) / 1000));
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}m ${secs}s`;
  };

  const viewSession = (session: InterviewSession) => {
    if (session.status === 'completed') {
      navigate(`/feedback/${session.id}`);
    } else {
      navigate(`/voice-interview/${session.id}`);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  const getIconComponent = (icon: string) => {
    const icons: { [key: string]: any } = {
      '💻': Code,
      '⚙️': Settings,
      '🚀': Rocket,
      '🔧': Server,
      '📊': Briefcase,
      '🎨': Palette,
    };
    const IconComponent = icons[icon] || Briefcase;
    return <IconComponent className="h-6 w-6" />;
  };

  return (
    <div className="min-h-screen bg-gradient-subtle">
      <header className="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold bg-gradient-primary bg-clip-text text-transparent">
              InterviewPrep AI
            </h1>
            <p className="text-sm text-muted-foreground">Welcome back, {profile?.full_name}!</p>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="text-sm font-medium">Credits Remaining</p>
              <Badge variant="outline" className="text-lg font-bold">
                {profile?.credits || 0}
              </Badge>
            </div>
            <Button onClick={handleLogout} variant="outline" size="icon">
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-3xl font-bold mb-2">Choose Your Interview</h2>
            <p className="text-muted-foreground">Select a role, manage saved custom jobs, or review your history.</p>
          </div>
          <Button
            onClick={() => navigate("/custom-job")}
            className="bg-gradient-accent hover:opacity-90 transition-smooth"
          >
            <Plus className="mr-2 h-4 w-4" />
            Create Custom Job
          </Button>
        </div>

        <Tabs defaultValue="roles" className="space-y-6">
          <TabsList className="gap-2">
            <TabsTrigger value="roles">Practice Roles</TabsTrigger>
            <TabsTrigger value="custom">Saved Jobs</TabsTrigger>
            <TabsTrigger value="history">Past Interviews</TabsTrigger>
          </TabsList>

          <TabsContent value="roles">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {jobRoles.map((role) => (
                <Card
                  key={role.id}
                  className="hover:shadow-primary transition-smooth cursor-pointer group"
                  onClick={() => startInterview(role.id)}
                >
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <div className="p-3 rounded-lg bg-primary/10 text-primary group-hover:bg-gradient-primary group-hover:text-primary-foreground transition-smooth">
                        {getIconComponent(role.icon)}
                      </div>
                      <Badge variant="secondary">{role.category}</Badge>
                    </div>
                    <CardTitle className="mt-4">{role.title}</CardTitle>
                    <CardDescription>{role.description}</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <div className="flex flex-wrap gap-2">
                      {role.skills.slice(0, 3).map((skill) => (
                        <Badge key={skill} variant="outline" className="text-xs">
                          {skill}
                        </Badge>
                      ))}
                      {role.skills.length > 3 && (
                        <Badge variant="outline" className="text-xs">
                          +{role.skills.length - 3} more
                        </Badge>
                      )}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </TabsContent>

          <TabsContent value="custom">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {customJobs.length === 0 ? (
                <div className="rounded-lg border border-dashed border-border p-8 text-center text-sm text-muted-foreground">
                  You have no saved custom job descriptions yet. Create one to practice specifically for your dream role.
                </div>
              ) : (
                customJobs.map((job) => (
                  <Card key={job.id} className="border-border">
                    <CardHeader>
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <CardTitle>{job.title}</CardTitle>
                          <CardDescription>{job.description}</CardDescription>
                        </div>
                        <Button
                          variant="outline"
                          size="icon"
                          onClick={() => deleteCustomJob(job.id)}
                          aria-label={`Delete ${job.title}`}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </CardHeader>
                    <CardContent>
                      <div className="flex flex-wrap gap-2">
                        {(job.requirements ?? []).slice(0, 4).map((req) => (
                          <Badge key={req} variant="outline" className="text-xs">
                            {req}
                          </Badge>
                        ))}
                      </div>
                      <div className="mt-4 flex flex-wrap gap-2">
                        <Button size="sm" variant="secondary" onClick={() => startInterview(job.id, true)}>
                          Start Interview
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                ))
              )}
            </div>
          </TabsContent>

          <TabsContent value="history">
            <Card className="mt-0">
              <CardHeader>
                <div className="flex items-center justify-between gap-4">
                  <div>
                    <CardTitle>Past Interviews</CardTitle>
                    <CardDescription>Review completed and in-progress sessions.</CardDescription>
                  </div>
                  <Badge variant="outline" className="uppercase text-xs">
                    {sessions.length} sessions
                  </Badge>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {sessions.length === 0 ? (
                  <div className="rounded-lg border border-dashed border-border p-8 text-center text-sm text-muted-foreground">
                    You have no past interviews yet. Start a new session to create your first one.
                  </div>
                ) : (
                  <div className="space-y-4">
                    {sessions.map((session) => {
                      const title = session.jobRole?.title || session.customJob?.title || 'Interview Session';
                      const statusLabel = session.status === 'completed' ? 'Completed' : session.status === 'in_progress' ? 'In progress' : session.status === 'resume_uploaded' ? 'Resume ready' : 'Started';
                      const overallScore = session.status === 'completed' ? Math.round(((session.resumeScore ?? 0) + (session.interviewScore ?? 0)) / 2) : undefined;
                      return (
                        <div key={session.id} className="rounded-xl border border-border p-4 sm:p-5">
                          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                            <div>
                              <p className="text-sm text-muted-foreground mb-1">{formatDateTime(session.createdAt)}</p>
                              <h3 className="text-lg font-semibold">{title}</h3>
                            </div>
                            <div className="flex items-center gap-2">
                              <Badge variant={session.status === 'completed' ? 'secondary' : 'outline'}>{statusLabel}</Badge>
                              {overallScore !== undefined && (
                                <Badge variant="outline">{overallScore}%</Badge>
                              )}
                            </div>
                          </div>
                          <div className="mt-3 grid gap-3 sm:grid-cols-3">
                            <div className="rounded-lg bg-secondary/5 p-3 text-sm">
                              <p className="font-medium">Duration</p>
                              <p className="text-muted-foreground">{formatDuration(session.createdAt, session.completedAt)}</p>
                            </div>
                            <div className="rounded-lg bg-secondary/5 p-3 text-sm">
                              <p className="font-medium">Resume score</p>
                              <p className="text-muted-foreground">{session.resumeScore ?? '—'}%</p>
                            </div>
                            <div className="rounded-lg bg-secondary/5 p-3 text-sm">
                              <p className="font-medium">Interview score</p>
                              <p className="text-muted-foreground">{session.interviewScore ?? '—'}%</p>
                            </div>
                          </div>
                          <div className="mt-4 flex flex-wrap gap-3">
                            <Button onClick={() => viewSession(session)} size="sm" variant="outline">
                              {session.status === 'completed' ? 'View Feedback' : 'Continue Interview'}
                            </Button>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        <Card className="mt-8 bg-gradient-primary text-primary-foreground">
          <CardHeader>
            <CardTitle>Need More Credits?</CardTitle>
            <CardDescription className="text-primary-foreground/80">
              Get more interview sessions to continue practicing
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button
              variant="secondary"
              onClick={() => navigate("/pricing")}
              className="font-semibold"
            >
              View Pricing
            </Button>
          </CardContent>
        </Card>
      </main>
    </div>
  );
};

export default Dashboard;
