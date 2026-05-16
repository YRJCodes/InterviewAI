using Interviewzwt.Backend.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

namespace Interviewzwt.Backend.Services
{
    public class AIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<ResumeAnalysisResponse> AnalyzeResume(string resumeText, string jobDescription, string jobTitle)
        {
            var apiKey = _configuration["ExternalAPIs:Groq:ApiKey"];
            var model = _configuration["ExternalAPIs:Groq:Model"] ?? "llama3-8b-8192";

            var cleanResume = CleanText(resumeText, 2000);
            var cleanJobDesc = CleanText(jobDescription, 2000);

            var prompt = $"Analyze this resume for the {jobTitle} position and provide a match score (0-100) and brief feedback (2-3 sentences).\n\nJob: {cleanJobDesc}\n\nResume: {cleanResume}\n\nRespond with JSON: {{\"score\": <number>, \"feedback\": \"<text>\"}}";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 300
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Groq Response:");
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Groq API Error ({(int)response.StatusCode}): {content}"
                );
            }
            var jsonDoc = JsonDocument.Parse(content);
            var aiText = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            var match = Regex.Match(aiText, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                return JsonSerializer.Deserialize<ResumeAnalysisResponse>(match.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                    ?? new ResumeAnalysisResponse { Score = 75, Feedback = "Resume reviewed successfully" };
            }

            return new ResumeAnalysisResponse { Score = 75, Feedback = aiText.Length > 200 ? aiText.Substring(0, 200) : aiText };
        }

        public async Task<string> TranscribeAudio(string base64Audio)
{
    var apiKey = _configuration["ExternalAPIs:AssemblyAI:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new Exception("AssemblyAI API key missing");

    var audioBytes = Convert.FromBase64String(base64Audio);

    // Upload audio
    var uploadRequest = new HttpRequestMessage(
        HttpMethod.Post,
        "https://api.assemblyai.com/v2/upload"
    );

    uploadRequest.Headers.Add("authorization", apiKey);

    uploadRequest.Content = new ByteArrayContent(audioBytes);

    uploadRequest.Content.Headers.ContentType =
        new MediaTypeHeaderValue("application/octet-stream");

    var uploadResponse = await _httpClient.SendAsync(uploadRequest);

    var uploadContent =
        await uploadResponse.Content.ReadAsStringAsync();

    Console.WriteLine(uploadContent);

    if (!uploadResponse.IsSuccessStatusCode)
    {
        throw new Exception(
            $"AssemblyAI Upload Error {(int)uploadResponse.StatusCode}: {uploadContent}"
        );
    }

    var uploadData = JsonDocument.Parse(uploadContent);

    var uploadUrl =
        uploadData.RootElement.GetProperty("upload_url").GetString();

    // Start transcription
    var transcriptRequest = new HttpRequestMessage(
        HttpMethod.Post,
        "https://api.assemblyai.com/v2/transcript"
    );

    transcriptRequest.Headers.Add("authorization", apiKey);

    transcriptRequest.Content = new StringContent(
    JsonSerializer.Serialize(new
    {
        audio_url = uploadUrl,
        speech_models = new[] { "universal-2" }
    }),
    Encoding.UTF8,
    "application/json"
);

    var transcriptResponse =
        await _httpClient.SendAsync(transcriptRequest);

    var transcriptContent =
        await transcriptResponse.Content.ReadAsStringAsync();

    Console.WriteLine(transcriptContent);

    if (!transcriptResponse.IsSuccessStatusCode)
    {
        throw new Exception(
            $"AssemblyAI Transcript Error {(int)transcriptResponse.StatusCode}: {transcriptContent}"
        );
    }

    var transcriptData = JsonDocument.Parse(transcriptContent);

    var id =
        transcriptData.RootElement.GetProperty("id").GetString();

    // Polling
    int attempts = 0;

    while (attempts < 30)
    {
        var pollRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.assemblyai.com/v2/transcript/{id}"
        );

        pollRequest.Headers.Add("authorization", apiKey);

        var pollResponse =
            await _httpClient.SendAsync(pollRequest);

        var pollContent =
            await pollResponse.Content.ReadAsStringAsync();

        Console.WriteLine(pollContent);

        var pollData = JsonDocument.Parse(pollContent);

        var status =
            pollData.RootElement.GetProperty("status").GetString();

        if (status == "completed")
        {
            return pollData.RootElement
                .GetProperty("text")
                .GetString() ?? "";
        }

        if (status == "error")
        {
            var error =
                pollData.RootElement.TryGetProperty("error", out var err)
                    ? err.GetString()
                    : "Unknown transcription error";

            throw new Exception(error);
        }

        await Task.Delay(1000);

        attempts++;
    }

    throw new Exception("Transcription timeout");
}

        public async Task<string> GenerateInterviewResponse(string jobDescription, List<AIMessage> messages)
        {
            var apiKey = _configuration["ExternalAPIs:Groq:ApiKey"];
            var model = _configuration["ExternalAPIs:Groq:Model"] ?? "llama3-8b-8192";

            var systemPrompt = $"You are an experienced interviewer conducting a professional job interview. \nJob Description: {jobDescription}\n\nGuidelines:\n- Ask relevant questions based on the job description\n- Provide constructive feedback\n- Be professional and encouraging\n- Keep responses concise (2-3 sentences)\n- Ask follow-up questions when appropriate";

            var apiMessages = new List<object> { new { role = "system", content = systemPrompt } };
            apiMessages.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

            var requestBody = new
            {
                model = model,
                messages = apiMessages,
                temperature = 0.8,
                max_tokens = 200
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        public async Task<InterviewScoreResponse> ScoreInterview(string conversationText, string jobDescription)
        {
            var apiKey = _configuration["ExternalAPIs:Groq:ApiKey"];
            var model = _configuration["ExternalAPIs:Groq:Model"] ?? "llama3-8b-8192";

            var cleanConversation = CleanText(conversationText, 3000);
            var cleanJobDesc = CleanText(jobDescription, 2000);

            var prompt = $"You are an expert interview evaluator. Score the candidate's performance based on their responses in this interview for the position: {cleanJobDesc}\n\nInterview Conversation:\n{cleanConversation}\n\nEvaluation Criteria:\n- 0-20: No responses or only AI greeting\n- 21-40: Very brief, unclear responses\n- 41-60: Basic responses, lacks depth\n- 61-80: Good responses, shows understanding\n- 81-100: Excellent responses, demonstrates expertise\n\nProvide:\n1. A performance score from 0-100 based STRICTLY on candidate responses quality and depth\n2. Brief feedback (2-3 sentences) highlighting strengths and areas for improvement\n\nRespond ONLY with valid JSON (no markdown): {{\"score\": <number 0-100>, \"feedback\": \"<string>\"}}";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.5,
                max_tokens = 300
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var aiText = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            var match = Regex.Match(aiText, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                return JsonSerializer.Deserialize<InterviewScoreResponse>(match.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                    ?? new InterviewScoreResponse { Score = 75, Feedback = "Interview completed" };
            }

            return new InterviewScoreResponse { Score = 75, Feedback = aiText.Length > 200 ? aiText.Substring(0, 200) : aiText };
        }

        private string CleanText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var cleaned = Regex.Replace(text, @"[^\x20-\x7E\n]", "");
            cleaned = Regex.Replace(cleaned, @"\s+", " ");
            return cleaned.Length > maxLength ? cleaned.Substring(0, maxLength) : cleaned;
        }
    }
}
