/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
    grunt.initConfig({
        //bower: {
        //    install: {
        //        options: {
        //            targetDir: "wwwroot/lib",
        //            layout: "byComponent",
        //            cleanTargetDir: false
        //        }
        //    }
        //},
        shell: {
            push: {
                command: [
                    'powershell .\\push-nuget.ps1'
                ].join('&& ')
            },
            build: {
                command: "dnu build"
            },
            build_dnx: {
                command: "dnu build --framework dnx451"
            },
            build_dnxcore: {
                command: "dnu build --framework dnxcore50"
            }
        }
    });

    grunt.registerTask("push-nuget", ["shell:push"]);
    grunt.registerTask("build", ["shell:build"]);
    grunt.registerTask("build-dnx", ["shell:build_dnx"]);
    grunt.registerTask("build-dnxcore", ["shell:build:dnxcore"]);

    //grunt.loadNpmTasks("grunt-bower-task");
    grunt.loadNpmTasks("grunt-shell");
    
};