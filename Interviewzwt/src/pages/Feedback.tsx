import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "@/integrations/api/client";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/hooks/use-toast";
import { Loader2, Trophy, TrendingUp, Home, FileText } from "lucide-react";

interface InterviewSession {
  id?: string;
  createdAt?: string;
  completedAt?: string;
  transcript?: string;
  resumeScore?: number;
  resume_score?: number;
  resumeFeedback?: string;
  resume_feedback?: string;
  interviewScore?: number;
  interview_score?: number;
  interviewFeedback?: string;
  interview_feedback?: string;
  jobRoleId?: any;
  customJobId?: any;
  jobRole?: {
    title: string;
  };
  customJob?: {
    title: string;
  };
  job_roles?: {
    title: string;
  };
}

const Feedback = () => {
  const { sessionId } = useParams();
  const [session, setSession] = useState<InterviewSession | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { toast } = useToast();

  useEffect(() => {
    fetchSession();
  }, [sessionId]);

  const fetchSession = async () => {
    try {
      await apiClient.me();
      const session = await apiClient.getSession(sessionId as string);
      console.log('Fetched session:', session);
      setSession(session);
    } catch (error: any) {
      console.error("Error fetching session:", error);
      setErrorMsg(error.message || "Unknown error occurred");
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!session) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Card>
          <CardHeader>
            <CardTitle>{errorMsg ? "Error Loading Session" : "Session Not Found"}</CardTitle>
            <CardDescription>{errorMsg ? `Details: ${errorMsg}` : "The interview session could not be found."}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => navigate("/dashboard")}>Return to Dashboard</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const resumeScore = session.resumeScore ?? session.resume_score ?? 0;
  const resumeFeedback = session.resumeFeedback ?? session.resume_feedback ?? '';
  const interviewScore = session.interviewScore ?? session.interview_score ?? 0;
  const interviewFeedback = session.interviewFeedback ?? session.interview_feedback ?? '';
  const createdAt = session.createdAt ? new Date(session.createdAt) : undefined;
  const completedAt = session.completedAt ? new Date(session.completedAt) : undefined;

  const jobTitle = session.jobRole?.title || session.customJob?.title || session.jobRoleId?.title || session.customJobId?.title || session.job_roles?.title || 'Interview';
  
  const overallScore = Math.round((resumeScore + interviewScore) / 2);
  const transcriptLines = session.transcript?.split('\n').filter(line => line.trim()) ?? [];
  const transcriptItems = transcriptLines.map((line) => {
    const [role, ...rest] = line.split(':');
    return {
      role: role?.trim() || 'Unknown',
      content: rest.join(':').trim(),
    };
  });

  const getScoreColor = (score: number) => {
    if (score >= 80) return "text-accent";
    if (score >= 60) return "text-primary";
    return "text-destructive";
  };

  const formatDateTime = (date?: Date) => {
    if (!date) return 'N/A';
    return date.toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  };

  const formatElapsed = (start?: Date, end?: Date) => {
    if (!start || !end) return 'In progress';
    const seconds = Math.max(0, Math.floor((end.getTime() - start.getTime()) / 1000));
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}m ${secs}s`;
  };

  return (
    <div className="min-h-screen bg-gradient-subtle">
      <header className="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container mx-auto px-4 py-4">
          <h1 className="text-2xl font-bold bg-gradient-primary bg-clip-text text-transparent">
            InterviewPrep AI
          </h1>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="mb-8">
          <Button variant="ghost" onClick={() => navigate("/dashboard")}>
            <Home className="mr-2 h-4 w-4" />
            Back to Dashboard
          </Button>
        </div>

        <Card className="mb-6 bg-gradient-primary text-primary-foreground">
          <CardHeader>
            <div className="flex items-center gap-3">
              <Trophy className="h-8 w-8" />
              <div>
                <CardTitle className="text-2xl">Interview Complete!</CardTitle>
                <CardDescription className="text-primary-foreground/80">
                  {jobTitle}
                </CardDescription>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-primary-foreground/80 mb-1">Overall Score</p>
                <p className={`text-5xl font-bold`}>
                  {overallScore}%
                </p>
              </div>
              <div className="text-right">
                <Badge variant="secondary" className="text-lg py-2 px-4">
                  {overallScore >= 80 ? "Excellent" : overallScore >= 60 ? "Good" : "Needs Improvement"}
                </Badge>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="grid md:grid-cols-2 gap-6 mb-6">
          <Card>
            <CardHeader>
              <div className="flex items-center gap-2">
                <FileText className="h-5 w-5 text-primary" />
                <CardTitle>Resume Analysis</CardTitle>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium">Match Score</span>
                  <span className={`text-2xl font-bold ${getScoreColor(resumeScore)}`}>
                    {resumeScore}%
                  </span>
                </div>
                <Progress value={resumeScore} className="h-2" />
              </div>
              <div>
                <h4 className="font-semibold mb-2 text-sm">Feedback</h4>
                <p className="text-sm text-muted-foreground">{resumeFeedback}</p>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center gap-2">
                <TrendingUp className="h-5 w-5 text-primary" />
                <CardTitle>Interview Performance</CardTitle>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium">Interview Score</span>
                  <span className={`text-2xl font-bold ${getScoreColor(interviewScore)}`}>
                    {interviewScore}%
                  </span>
                </div>
                <Progress value={interviewScore} className="h-2" />
              </div>
              <div>
                <h4 className="font-semibold mb-2 text-sm">Feedback</h4>
                <p className="text-sm text-muted-foreground">{interviewFeedback}</p>
              </div>
            </CardContent>
          </Card>
        </div>

        <Card className="mb-6">
          <CardHeader>
            <div className="flex items-center justify-between gap-4">
              <div>
                <CardTitle>Session Details</CardTitle>
                <CardDescription>Review when this interview ended and how long it lasted.</CardDescription>
              </div>
              <Badge variant="outline" className="uppercase text-xs">
                {completedAt ? 'Completed' : 'In progress'}
              </Badge>
            </div>
          </CardHeader>
          <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="rounded-lg bg-secondary/5 p-4 text-sm">
              <p className="font-medium">Started</p>
              <p className="text-muted-foreground">{formatDateTime(createdAt)}</p>
            </div>
            <div className="rounded-lg bg-secondary/5 p-4 text-sm">
              <p className="font-medium">Ended</p>
              <p className="text-muted-foreground">{formatDateTime(completedAt)}</p>
            </div>
            <div className="rounded-lg bg-secondary/5 p-4 text-sm md:col-span-2">
              <p className="font-medium">Duration</p>
              <p className="text-muted-foreground">{formatElapsed(createdAt, completedAt)}</p>
            </div>
          </CardContent>
        </Card>

        {transcriptItems.length > 0 && (
          <Card className="mb-6">
            <CardHeader>
              <CardTitle>Question & answer history</CardTitle>
              <CardDescription>Review the full interview transcript.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {transcriptItems.map((item, idx) => (
                <div key={`${item.role}-${idx}`} className={`rounded-xl p-4 ${item.role.toLowerCase().includes('assistant') ? 'bg-secondary/5' : 'bg-primary/5'}`}>
                  <p className="text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground mb-1">
                    {item.role === 'assistant' || item.role === 'ai' ? 'Question' : 'Answer'}
                  </p>
                  <p className="text-sm">{item.content}</p>
                </div>
              ))}
            </CardContent>
          </Card>
        )}

        <Card>
          <CardHeader>
            <CardTitle>Next Steps</CardTitle>
            <CardDescription>Continue improving your interview skills</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid md:grid-cols-2 gap-4">
              <Button
                onClick={() => navigate("/dashboard")}
                className="bg-gradient-primary hover:opacity-90"
              >
                Practice Another Role
              </Button>
              <Button
                onClick={() => navigate("/pricing")}
                variant="outline"
              >
                Get More Credits
              </Button>
            </div>
          </CardContent>
        </Card>
      </main>
    </div>
  );
};

export default Feedback;
