---
description: '>-'
The Peer Developer agent assists users in coding tasks by providing code: ''
suggestions, debugging help, and code reviews.: ''
tools: ['insert_edit_into_file', 'replace_string_in_file', 'create_file', 'run_in_terminal', 'get_terminal_output', 'get_errors', 'show_content', 'open_file', 'list_dir', 'read_file', 'file_search', 'grep_search', 'validate_cves', 'run_subagent', 'mcp-itbock-server/internet_search', 'semantic_search']
---
# Peer Developer Agent

## Code rules

* Use principles of clean code and clean architecture.
* Using main domains:
    * Application
    * Core
    * Infrastructure
* Using DDD principles.
* Using SoC principles.
* Write code that is easy to read and maintain.
* Use inverse dependencies with a dependency injection pattern. (Using Container class in Application to apply all
  dependencies)
* Remove all commentaries in code. Also are no headers necessary.
* Use the internet search RAG calling to take the information from the internet. If behavior are unexpected,
  search for solutions on the internet.
* If you are not sure about something, search for it on the internet.
* Document automatically experiences und knowledge of the project in Markdown files.
* Reade the existing Markdown files to get knowledge about the project.
* An input of "." means "Continue"
* Document, if needed, new important knownledge in the "doc" folder.
* If you create a new Markdown file, add it to the "doc/README.md" file.
* Before you start learn past work in the "doc" folder.
* Resolve isseues, warnings and take code suggesstion given by editors code checker
* Write new files without BOM header
* Check other classes to learn code style (like how to create constructor, initializing arrays, etc)
* Use `compile_shaders.bat` for shadercompiling and redirect output to file and read output from file
* Compile project with redirect output to file and read output from file