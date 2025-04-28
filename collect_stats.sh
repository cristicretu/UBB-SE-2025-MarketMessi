#!/bin/bash

echo "Starting contribution stats collection..."

# Define output CSV file
csv_file="git_contribution_stats.csv"
echo "Author,Total Insertions,Total Deletions,Unique Files,Total Commits,Total Lines Changed" > "$csv_file"

# Create temp directory for stats
temp_dir=$(mktemp -d)
touch "$temp_dir/author_stats.tmp"

# Set cutoff date to April 15, 2025
CUTOFF_DATE="2025-04-15"

# Create an awk script for normalizing author names
cat > "$temp_dir/normalize.awk" << 'EOT'
BEGIN {
  authors["Cristian Crețu <crisemcr@gmail.com>"] = "Cristian Cretu"
  authors["Cristian Cretu <crisemcr@gmail.com>"] = "Cristian Cretu"
  authors["Mihai-Cristian Farcaș <146859576+Forquosh@users.noreply.github.com>"] = "Mihai-Cristian Farcaș"
  authors["Mihai-Cristian Farcaș <mihaifarcas125@gmail.com>"] = "Mihai-Cristian Farcaș"
  authors["Grancea Alexandru <103886681+Nanu25@users.noreply.github.com>"] = "Grancea Alexandru"
  authors["Nanu25 <alexandrugrancea25@gmail.com>"] = "Grancea Alexandru"
  authors["cretu-luca <123098894+cretu-luca@users.noreply.github.com>"] = "Luca Cretu"
  authors["luca <lucac375@gmail.com>"] = "Luca Cretu"
  authors["Vasile Draguța <141652021+vasile-draguta@users.noreply.github.com>"] = "Vasile Draguta"
  authors["Vasile Draguta <dragutavasile@gmail.com>"] = "Vasile Draguta"
}
{
  if ($0 in authors)
    print authors[$0]
  else
    print $0
}
EOT

# Get only origin/main branch for simplicity
echo "Processing only origin/main branch for accurate stats"
branches="origin/main"
original_branch=$(git rev-parse --abbrev-ref HEAD)

# Function to normalize author name
normalize_author() {
    echo "$1" | awk -f "$temp_dir/normalize.awk"
}

# Binary files and directories to ignore
cat > "$temp_dir/ignore_patterns.txt" << EOT
planning/
.jpg
.png
.pdf
.lock
.bin
node_modules/
.git/
EOT

# Checkout the main branch
echo "Checking out main branch"
git checkout origin/main &>/dev/null

# Get unique list of files for reference
git ls-files > "$temp_dir/all_files.txt"
echo "Found $(wc -l < "$temp_dir/all_files.txt" | tr -d ' ') files in repo"

# Get unique files per author
echo "Collecting file stats per author..."
git log --since="$CUTOFF_DATE" --pretty=format:"COMMIT_START %h %an <%ae>" --name-status | awk -v temp_dir="$temp_dir" '
BEGIN { 
    author = ""; 
    commit = "";
    
    # Read ignore patterns
    while((getline pattern < (temp_dir "/ignore_patterns.txt")) > 0) {
        ignore_patterns[pattern] = 1;
    }
    close(temp_dir "/ignore_patterns.txt");
}

# New commit starts
/^COMMIT_START/ {
    commit = $2;
    raw_author = substr($0, index($0, $3));
    
    # Normalize author
    cmd = "echo \"" raw_author "\" | awk -f \"" temp_dir "/normalize.awk\"";
    cmd | getline author;
    close(cmd);
    next;
}

# File line (starts with M/A/D)
author != "" && /^[MAD]/ {
    status = $1;
    file = substr($0, 2);
    sub(/^[ \t]+/, "", file);  # Trim leading spaces
    
    # Skip files matching ignore patterns
    skip = 0;
    for (pattern in ignore_patterns) {
        if (index(file, pattern) > 0) {
            skip = 1;
            break;
        }
    }
    if (skip) next;
    
    # Use a string key to track unique files per author
    author_file_key = author ":" file;
    if (!(author_file_key in seen_files)) {
        seen_files[author_file_key] = 1;
        # Count unique files per author
        file_count[author]++;
    }
    
    # Mark this author as having commits
    has_commits[author] = 1;
}

END {
    # Output the file counts per author
    for (a in has_commits) {
        printf "%s,%d\n", a, file_count[a];
    }
}' > "$temp_dir/file_counts.tmp"

# Get insertions and deletions per author
echo "Collecting line stats per author..."
for author in $(awk -F, '{print $1}' "$temp_dir/file_counts.tmp" | sort -u); do
    # Get insertions/deletions for this author
    git log --author="$author" --since="$CUTOFF_DATE" --pretty=tformat: --numstat | awk -v author="$author" -v temp_dir="$temp_dir" '
    BEGIN {
        insertions = 0;
        deletions = 0;
        
        # Read ignore patterns
        while((getline pattern < (temp_dir "/ignore_patterns.txt")) > 0) {
            ignore_patterns[pattern] = 1;
        }
        close(temp_dir "/ignore_patterns.txt");
    }
    
    NF >= 3 { 
        ins = $1;
        del = $2;
        file = $3;
        
        # Skip binary files and ignored patterns
        if (ins == "-" || del == "-") next;
        
        skip = 0;
        for (pattern in ignore_patterns) {
            if (index(file, pattern) > 0) {
                skip = 1;
                break;
            }
        }
        if (skip) next;
        
        insertions += ins;
        deletions += del;
    }
    
    END {
        printf "%s,%d,%d\n", author, insertions, deletions;
    }' >> "$temp_dir/author_stats.tmp"
done

# Get commit counts
echo "Counting commits per author..."
git shortlog -s -n --since="$CUTOFF_DATE" | while read -r line; do
    count=$(echo "$line" | awk '{print $1}')
    raw_author=$(echo "$line" | sed 's/^\s*[0-9]\+\s\+//')
    
    # Normalize author
    author=$(normalize_author "$raw_author")
    echo "$author,$count" >> "$temp_dir/commit_counts.tmp"
done

# Return to original branch
git checkout $original_branch
echo "Returned to original branch: $original_branch"

# Consolidate stats
echo "Calculating final totals..."

# Create final stats by joining all data
awk -F, '
    # Load file counts
    FILENAME == ARGV[1] {
        files[$1] = $2;
        next;
    }
    
    # Load commit counts
    FILENAME == ARGV[2] && NF == 2 {
        commits[$1] = $2;
        next;
    }
    
    # Process line stats and create final output
    FILENAME == ARGV[3] && NF >= 3 {
        author = $1;
        insertions = $2;
        deletions = $3;
        
        file_count = files[$1] ? files[$1] : 0;
        commit_count = commits[$1] ? commits[$1] : 0;
        lines_changed = insertions + deletions;
        
        # Skip entries with author starting with "-"
        if (author ~ /^-/) next;
        
        printf "%s,%d,%d,%d,%d,%d\n", author, insertions, deletions, file_count, commit_count, lines_changed;
    }
' "$temp_dir/file_counts.tmp" "$temp_dir/commit_counts.tmp" "$temp_dir/author_stats.tmp" | sort -t, -k6,6nr >> "$csv_file"

echo "Stats collection complete!"
echo "Results saved to: $csv_file"
echo "You can preview it with: column -s, -t $csv_file"

# Clean up
rm -rf "$temp_dir"
