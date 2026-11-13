# Copilot Instructions

## Project Guidelines
- For the QnA Intelligence roadmap, the user chose Azure OpenAI notification-email ingestion rather than scraping Microsoft Q&A, and plans to implement automatic question analysis as a background job using Hangfire after request logging.
- For the QnA Intelligence dashboard status filter, the user chose to replace QuestionStatus.Notified with Answered, preserving the enum numeric value 3 for database compatibility.

## Question Processing
- For question imports, process each new question sequentially: persist Question/QuestionSource, call IAiAnalysisService, persist QuestionAnalysis and set status Analyzed, then proceed to the next candidate.
- Skip duplicates without analysis, continue processing later candidates after an individual analysis failure, and retain the existing manual Analyze endpoint.