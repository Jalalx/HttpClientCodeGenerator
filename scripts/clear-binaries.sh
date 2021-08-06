cd ..

# Deletes all bin & obj folders
find . -iname "bin" -print0 | xargs -0 rm -rf
find . -iname "obj" -print0 | xargs -0 rm -rf

# Kill VS roslyn service
#taskkill /IM "servicehub.roslyncodeanalysisservice" /F

# Clear cache
#rm "%TEMP%\VS\AnalyzerAssemblyLoader -r"