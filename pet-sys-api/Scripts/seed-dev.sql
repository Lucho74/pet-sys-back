-- Datos de prueba para la base de DEV (Azure SQL).
-- 105 usuarios (5 Admin, 15 Veterinarian, 85 Client), 20 mascotas y 40 consultas.
-- Todos los usuarios tienen la contraseña: Password123!  (guardada como hash BCrypt)
--
-- Requisitos: la base tiene que tener el esquema creado (migraciones aplicadas; lo hace el workflow de deploy).
-- Si la tabla Users ya tiene filas, el script no inserta nada.
-- Las fechas de las consultas son relativas al día en que se corre (con hora entre las 9 y las 18).
-- Si cambian las entidades/migraciones, este script se tiene que actualizar a mano.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM [dbo].[Users])
BEGIN
    PRINT 'La tabla Users ya tiene datos. No se insertó nada (vaciá las tablas si querés volver a sembrar).';
    RETURN;
END

DECLARE @PasswordHash nvarchar(100) = N'$2a$11$5KEaPrjkfCBCdqJ5paTtY.B6/LGUXctDra8hSQFI.MLseq0xqpxKy';
DECLARE @Today date = CAST(GETDATE() AS date);

BEGIN TRANSACTION;

SET IDENTITY_INSERT [dbo].[Users] ON;
INSERT INTO [dbo].[Users] ([Id], [FullName], [Password], [Email], [Phone], [IsDeleted], [UserType], [Dni]) VALUES
    (1, N'Camila Sánchez', @PasswordHash, N'admin1@4patas.com', N'341-452-7489', 0, N'Admin', NULL),
    (2, N'Agustina López', @PasswordHash, N'admin2@4patas.com', N'341-446-2089', 0, N'Admin', NULL),
    (3, N'Juan Álvarez', @PasswordHash, N'admin3@4patas.com', N'341-681-5741', 0, N'Admin', NULL),
    (4, N'Rocío Molina', @PasswordHash, N'admin4@4patas.com', N'341-430-4636', 0, N'Admin', NULL),
    (5, N'Facundo Benítez', @PasswordHash, N'admin5@4patas.com', N'341-584-5533', 0, N'Admin', NULL),
    (6, N'Ignacio Díaz', @PasswordHash, N'veterinario1@4patas.com', N'341-454-5288', 0, N'Veterinarian', NULL),
    (7, N'Mateo Núñez', @PasswordHash, N'veterinario2@4patas.com', N'341-413-5264', 0, N'Veterinarian', NULL),
    (8, N'Rocío García', @PasswordHash, N'veterinario3@4patas.com', N'341-499-3700', 0, N'Veterinarian', NULL),
    (9, N'Lucía Sánchez', @PasswordHash, N'veterinario4@4patas.com', N'341-590-2420', 0, N'Veterinarian', NULL),
    (10, N'Antonella Medina', @PasswordHash, N'veterinario5@4patas.com', N'341-572-7355', 0, N'Veterinarian', NULL),
    (11, N'Facundo Pérez', @PasswordHash, N'veterinario6@4patas.com', N'341-491-5052', 0, N'Veterinarian', NULL),
    (12, N'Agustina García', @PasswordHash, N'veterinario7@4patas.com', N'341-445-9972', 0, N'Veterinarian', NULL),
    (13, N'Franco Sánchez', @PasswordHash, N'veterinario8@4patas.com', N'341-403-5783', 0, N'Veterinarian', NULL),
    (14, N'Federico Aguirre', @PasswordHash, N'veterinario9@4patas.com', N'341-559-9329', 0, N'Veterinarian', NULL),
    (15, N'Mateo Torres', @PasswordHash, N'veterinario10@4patas.com', N'341-616-5721', 0, N'Veterinarian', NULL),
    (16, N'Florencia Ruiz', @PasswordHash, N'veterinario11@4patas.com', N'341-482-4821', 0, N'Veterinarian', NULL),
    (17, N'Lucía García', @PasswordHash, N'veterinario12@4patas.com', N'341-422-2328', 0, N'Veterinarian', NULL),
    (18, N'María Ruiz', @PasswordHash, N'veterinario13@4patas.com', N'341-543-9501', 0, N'Veterinarian', NULL),
    (19, N'Julieta Herrera', @PasswordHash, N'veterinario14@4patas.com', N'341-641-6617', 0, N'Veterinarian', NULL),
    (20, N'Martín Suárez', @PasswordHash, N'veterinario15@4patas.com', N'341-500-2088', 0, N'Veterinarian', NULL),
    (21, N'Florencia Núñez', @PasswordHash, N'cliente1@mail.com', N'341-503-8227', 0, N'Client', N'30097531'),
    (22, N'Santiago Díaz', @PasswordHash, N'cliente2@mail.com', N'341-582-8142', 0, N'Client', N'30195062'),
    (23, N'Micaela Acosta', @PasswordHash, N'cliente3@mail.com', N'341-564-4255', 0, N'Client', N'30292593'),
    (24, N'Emiliano Romero', @PasswordHash, N'cliente4@mail.com', N'341-451-2009', 0, N'Client', N'30390124'),
    (25, N'Diego Pérez', @PasswordHash, N'cliente5@mail.com', N'341-542-4889', 0, N'Client', N'30487655'),
    (26, N'Sofía Romero', @PasswordHash, N'cliente6@mail.com', N'341-490-5766', 0, N'Client', N'30585186'),
    (27, N'Nicolás González', @PasswordHash, N'cliente7@mail.com', N'341-421-6850', 0, N'Client', N'30682717'),
    (28, N'Diego Gómez', @PasswordHash, N'cliente8@mail.com', N'341-546-6359', 0, N'Client', N'30780248'),
    (29, N'Juan Romero', @PasswordHash, N'cliente9@mail.com', N'341-547-6270', 0, N'Client', N'30877779'),
    (30, N'Martín Molina', @PasswordHash, N'cliente10@mail.com', N'341-610-2273', 0, N'Client', N'30975310'),
    (31, N'Lucía Medina', @PasswordHash, N'cliente11@mail.com', N'341-498-8276', 0, N'Client', N'31072841'),
    (32, N'Lucía López', @PasswordHash, N'cliente12@mail.com', N'341-528-7252', 0, N'Client', N'31170372'),
    (33, N'Carolina Díaz', @PasswordHash, N'cliente13@mail.com', N'341-569-1153', 0, N'Client', N'31267903'),
    (34, N'Martina Rodríguez', @PasswordHash, N'cliente14@mail.com', N'341-632-3778', 0, N'Client', N'31365434'),
    (35, N'Martina Castro', @PasswordHash, N'cliente15@mail.com', N'341-585-5756', 0, N'Client', N'31462965'),
    (36, N'Federico Fernández', @PasswordHash, N'cliente16@mail.com', N'341-624-4395', 0, N'Client', N'31560496'),
    (37, N'Florencia Núñez', @PasswordHash, N'cliente17@mail.com', N'341-506-2861', 0, N'Client', N'31658027'),
    (38, N'María Rodríguez', @PasswordHash, N'cliente18@mail.com', N'341-428-3763', 0, N'Client', N'31755558'),
    (39, N'Carolina Suárez', @PasswordHash, N'cliente19@mail.com', N'341-476-1669', 0, N'Client', N'31853089'),
    (40, N'Julieta Ramírez', @PasswordHash, N'cliente20@mail.com', N'341-698-5080', 0, N'Client', N'31950620'),
    (41, N'Joaquín Rodríguez', @PasswordHash, N'cliente21@mail.com', N'341-462-9671', 0, N'Client', N'32048151'),
    (42, N'Lucía Molina', @PasswordHash, N'cliente22@mail.com', N'341-609-4281', 0, N'Client', N'32145682'),
    (43, N'Agustina Martínez', @PasswordHash, N'cliente23@mail.com', N'341-523-8189', 0, N'Client', N'32243213'),
    (44, N'Florencia Ramírez', @PasswordHash, N'cliente24@mail.com', N'341-418-4589', 0, N'Client', N'32340744'),
    (45, N'Florencia Ruiz', @PasswordHash, N'cliente25@mail.com', N'341-527-8009', 0, N'Client', N'32438275'),
    (46, N'Franco Martínez', @PasswordHash, N'cliente26@mail.com', N'341-655-4075', 0, N'Client', N'32535806'),
    (47, N'María Rodríguez', @PasswordHash, N'cliente27@mail.com', N'341-530-5151', 0, N'Client', N'32633337'),
    (48, N'Camila Flores', @PasswordHash, N'cliente28@mail.com', N'341-506-4792', 0, N'Client', N'32730868'),
    (49, N'Florencia Silva', @PasswordHash, N'cliente29@mail.com', N'341-533-3321', 0, N'Client', N'32828399'),
    (50, N'Joaquín Rodríguez', @PasswordHash, N'cliente30@mail.com', N'341-561-2914', 0, N'Client', N'32925930'),
    (51, N'Federico Álvarez', @PasswordHash, N'cliente31@mail.com', N'341-420-9098', 0, N'Client', N'33023461'),
    (52, N'Tomás Gómez', @PasswordHash, N'cliente32@mail.com', N'341-620-4455', 0, N'Client', N'33120992'),
    (53, N'Milagros Silva', @PasswordHash, N'cliente33@mail.com', N'341-693-3710', 0, N'Client', N'33218523'),
    (54, N'Joaquín Sánchez', @PasswordHash, N'cliente34@mail.com', N'341-641-6161', 0, N'Client', N'33316054'),
    (55, N'Franco Torres', @PasswordHash, N'cliente35@mail.com', N'341-670-4526', 0, N'Client', N'33413585'),
    (56, N'Gonzalo Castro', @PasswordHash, N'cliente36@mail.com', N'341-537-6546', 0, N'Client', N'33511116'),
    (57, N'Milagros Álvarez', @PasswordHash, N'cliente37@mail.com', N'341-654-2218', 0, N'Client', N'33608647'),
    (58, N'Antonella García', @PasswordHash, N'cliente38@mail.com', N'341-497-1729', 0, N'Client', N'33706178'),
    (59, N'Tomás Rojas', @PasswordHash, N'cliente39@mail.com', N'341-465-5414', 0, N'Client', N'33803709'),
    (60, N'Paula Silva', @PasswordHash, N'cliente40@mail.com', N'341-430-3740', 0, N'Client', N'33901240'),
    (61, N'Diego Herrera', @PasswordHash, N'cliente41@mail.com', N'341-637-8736', 0, N'Client', N'33998771'),
    (62, N'Micaela Álvarez', @PasswordHash, N'cliente42@mail.com', N'341-599-4580', 0, N'Client', N'34096302'),
    (63, N'Rocío González', @PasswordHash, N'cliente43@mail.com', N'341-508-3568', 0, N'Client', N'34193833'),
    (64, N'Juan Medina', @PasswordHash, N'cliente44@mail.com', N'341-531-2898', 0, N'Client', N'34291364'),
    (65, N'Tomás Castro', @PasswordHash, N'cliente45@mail.com', N'341-595-4642', 0, N'Client', N'34388895'),
    (66, N'Julieta Rodríguez', @PasswordHash, N'cliente46@mail.com', N'341-503-3652', 0, N'Client', N'34486426'),
    (67, N'Paula Medina', @PasswordHash, N'cliente47@mail.com', N'341-569-8725', 0, N'Client', N'34583957'),
    (68, N'Milagros Flores', @PasswordHash, N'cliente48@mail.com', N'341-625-1432', 0, N'Client', N'34681488'),
    (69, N'Lucas Rodríguez', @PasswordHash, N'cliente49@mail.com', N'341-457-9009', 0, N'Client', N'34779019'),
    (70, N'Julieta Silva', @PasswordHash, N'cliente50@mail.com', N'341-531-3273', 0, N'Client', N'34876550'),
    (71, N'María Sosa', @PasswordHash, N'cliente51@mail.com', N'341-440-9568', 0, N'Client', N'34974081'),
    (72, N'Emiliano González', @PasswordHash, N'cliente52@mail.com', N'341-552-6683', 0, N'Client', N'35071612'),
    (73, N'Franco Gómez', @PasswordHash, N'cliente53@mail.com', N'341-443-9902', 0, N'Client', N'35169143'),
    (74, N'Nicolás Álvarez', @PasswordHash, N'cliente54@mail.com', N'341-505-6098', 0, N'Client', N'35266674'),
    (75, N'Tomás Pérez', @PasswordHash, N'cliente55@mail.com', N'341-649-7554', 0, N'Client', N'35364205'),
    (76, N'Sofía Gómez', @PasswordHash, N'cliente56@mail.com', N'341-458-6994', 0, N'Client', N'35461736'),
    (77, N'Facundo Torres', @PasswordHash, N'cliente57@mail.com', N'341-612-8278', 0, N'Client', N'35559267'),
    (78, N'Lucas Herrera', @PasswordHash, N'cliente58@mail.com', N'341-500-5952', 0, N'Client', N'35656798'),
    (79, N'Milagros Ortiz', @PasswordHash, N'cliente59@mail.com', N'341-645-7921', 0, N'Client', N'35754329'),
    (80, N'Sofía Castro', @PasswordHash, N'cliente60@mail.com', N'341-685-3742', 0, N'Client', N'35851860'),
    (81, N'Martina Silva', @PasswordHash, N'cliente61@mail.com', N'341-483-3889', 0, N'Client', N'35949391'),
    (82, N'Diego López', @PasswordHash, N'cliente62@mail.com', N'341-567-9097', 0, N'Client', N'36046922'),
    (83, N'Antonella Romero', @PasswordHash, N'cliente63@mail.com', N'341-532-9877', 0, N'Client', N'36144453'),
    (84, N'Emiliano González', @PasswordHash, N'cliente64@mail.com', N'341-486-1091', 0, N'Client', N'36241984'),
    (85, N'Gonzalo Sánchez', @PasswordHash, N'cliente65@mail.com', N'341-461-9928', 0, N'Client', N'36339515'),
    (86, N'Sofía Ramírez', @PasswordHash, N'cliente66@mail.com', N'341-647-9623', 0, N'Client', N'36437046'),
    (87, N'Lucas Flores', @PasswordHash, N'cliente67@mail.com', N'341-525-7736', 0, N'Client', N'36534577'),
    (88, N'Antonella Sánchez', @PasswordHash, N'cliente68@mail.com', N'341-582-4755', 0, N'Client', N'36632108'),
    (89, N'Ignacio Díaz', @PasswordHash, N'cliente69@mail.com', N'341-400-1867', 0, N'Client', N'36729639'),
    (90, N'Carolina Núñez', @PasswordHash, N'cliente70@mail.com', N'341-560-9914', 0, N'Client', N'36827170'),
    (91, N'Milagros Núñez', @PasswordHash, N'cliente71@mail.com', N'341-639-6007', 0, N'Client', N'36924701'),
    (92, N'Milagros Silva', @PasswordHash, N'cliente72@mail.com', N'341-659-8221', 0, N'Client', N'37022232'),
    (93, N'Carolina Medina', @PasswordHash, N'cliente73@mail.com', N'341-626-7419', 0, N'Client', N'37119763'),
    (94, N'Martín García', @PasswordHash, N'cliente74@mail.com', N'341-585-6571', 0, N'Client', N'37217294'),
    (95, N'Martín Torres', @PasswordHash, N'cliente75@mail.com', N'341-442-3358', 0, N'Client', N'37314825'),
    (96, N'Paula Castro', @PasswordHash, N'cliente76@mail.com', N'341-490-5675', 0, N'Client', N'37412356'),
    (97, N'Emiliano Sosa', @PasswordHash, N'cliente77@mail.com', N'341-501-6741', 0, N'Client', N'37509887'),
    (98, N'Paula Medina', @PasswordHash, N'cliente78@mail.com', N'341-447-2262', 0, N'Client', N'37607418'),
    (99, N'Tomás Herrera', @PasswordHash, N'cliente79@mail.com', N'341-491-6400', 0, N'Client', N'37704949'),
    (100, N'Gonzalo Sosa', @PasswordHash, N'cliente80@mail.com', N'341-567-3861', 0, N'Client', N'37802480'),
    (101, N'Lucía Rojas', @PasswordHash, N'cliente81@mail.com', N'341-411-1329', 0, N'Client', N'37900011'),
    (102, N'Facundo Molina', @PasswordHash, N'cliente82@mail.com', N'341-445-6882', 0, N'Client', N'37997542'),
    (103, N'Rocío Ortiz', @PasswordHash, N'cliente83@mail.com', N'341-450-3596', 0, N'Client', N'38095073'),
    (104, N'Valentina Acosta', @PasswordHash, N'cliente84@mail.com', N'341-653-2259', 0, N'Client', N'38192604'),
    (105, N'Ignacio Núñez', @PasswordHash, N'cliente85@mail.com', N'341-459-3818', 0, N'Client', N'38290135');
SET IDENTITY_INSERT [dbo].[Users] OFF;

SET IDENTITY_INSERT [dbo].[Pets] ON;
INSERT INTO [dbo].[Pets] ([Id], [Name], [Specie], [Breed], [BirthDate], [ClientId]) VALUES
    (1, N'Luna', N'Ave', N'Cacatúa', '2021-08-13', 30),
    (2, N'Simón', N'Ave', N'Canario', '2013-03-01', 36),
    (3, N'Toby', N'Gato', N'Maine Coon', '2019-08-28', 28),
    (4, N'Mía', N'Conejo', N'Cabeza de León', '2014-07-21', 37),
    (5, N'Rocco', N'Ave', N'Cacatúa', '2017-08-16', 33),
    (6, N'Lola', N'Conejo', N'Rex', '2020-03-27', 34),
    (7, N'Milo', N'Perro', N'Golden Retriever', '2020-12-18', 38),
    (8, N'Nala', N'Ave', N'Cacatúa', '2014-01-06', 32),
    (9, N'Coco', N'Ave', N'Cacatúa', '2017-11-19', 21),
    (10, N'Bruno', N'Gato', N'Siamés', '2015-02-18', 35),
    (11, N'Kiara', N'Conejo', N'Rex', '2013-01-21', 23),
    (12, N'Thor', N'Conejo', N'Belier', '2015-10-04', 36),
    (13, N'Olivia', N'Perro', N'Golden Retriever', '2023-04-07', 23),
    (14, N'Felipe', N'Conejo', N'Belier', '2023-08-17', 26),
    (15, N'Frida', N'Ave', N'Cacatúa', '2013-07-20', 40),
    (16, N'Manchita', N'Conejo', N'Cabeza de León', '2026-03-09', 28),
    (17, N'Pancho', N'Gato', N'Persa', '2024-06-06', 22),
    (18, N'Chispa', N'Conejo', N'Belier', '2024-10-05', 29),
    (19, N'Oreo', N'Perro', N'Caniche', '2012-03-03', 40),
    (20, N'Tango', N'Gato', N'Europeo', '2023-04-03', 25);
SET IDENTITY_INSERT [dbo].[Pets] OFF;

SET IDENTITY_INSERT [dbo].[Consultations] ON;
INSERT INTO [dbo].[Consultations] ([Id], [Description], [Date], [PetId], [VeterinarianId]) VALUES
    (1, N'Análisis de sangre de rutina.', DATEADD(HOUR, 10, CAST(DATEADD(DAY, 44, @Today) AS datetime2)), 3, 14),
    (2, N'Análisis de sangre de rutina.', DATEADD(HOUR, 11, CAST(DATEADD(DAY, -87, @Today) AS datetime2)), 18, 8),
    (3, N'Desparasitación interna y externa.', DATEADD(HOUR, 12, CAST(DATEADD(DAY, -52, @Today) AS datetime2)), 7, 19),
    (4, N'Consulta por tos persistente.', DATEADD(HOUR, 13, CAST(DATEADD(DAY, -104, @Today) AS datetime2)), 3, 12),
    (5, N'Revisión de herida en pata trasera.', DATEADD(HOUR, 14, CAST(DATEADD(DAY, -137, @Today) AS datetime2)), 17, 12),
    (6, N'Limpieza dental y control de encías.', DATEADD(HOUR, 15, CAST(DATEADD(DAY, -43, @Today) AS datetime2)), 11, 8),
    (7, N'Limpieza dental y control de encías.', DATEADD(HOUR, 16, CAST(DATEADD(DAY, -81, @Today) AS datetime2)), 1, 10),
    (8, N'Evaluación por cojera en pata delantera.', DATEADD(HOUR, 17, CAST(DATEADD(DAY, 44, @Today) AS datetime2)), 1, 10),
    (9, N'Desparasitación interna y externa.', DATEADD(HOUR, 18, CAST(DATEADD(DAY, 22, @Today) AS datetime2)), 3, 8),
    (10, N'Control anual y vacunación antirrábica.', DATEADD(HOUR, 9, CAST(DATEADD(DAY, -151, @Today) AS datetime2)), 8, 9),
    (11, N'Control de peso y plan de alimentación.', DATEADD(HOUR, 10, CAST(DATEADD(DAY, -39, @Today) AS datetime2)), 18, 8),
    (12, N'Control de peso y plan de alimentación.', DATEADD(HOUR, 11, CAST(DATEADD(DAY, 46, @Today) AS datetime2)), 13, 11),
    (13, N'Desparasitación interna y externa.', DATEADD(HOUR, 12, CAST(DATEADD(DAY, 36, @Today) AS datetime2)), 17, 15),
    (14, N'Control de peso y plan de alimentación.', DATEADD(HOUR, 13, CAST(DATEADD(DAY, -132, @Today) AS datetime2)), 20, 20),
    (15, N'Consulta por picazón y caída de pelo.', DATEADD(HOUR, 14, CAST(DATEADD(DAY, 53, @Today) AS datetime2)), 10, 20),
    (16, N'Aplicación de vacuna séxtuple.', DATEADD(HOUR, 15, CAST(DATEADD(DAY, -15, @Today) AS datetime2)), 2, 19),
    (17, N'Revisión de herida en pata trasera.', DATEADD(HOUR, 16, CAST(DATEADD(DAY, -19, @Today) AS datetime2)), 19, 19),
    (18, N'Evaluación por cojera en pata delantera.', DATEADD(HOUR, 17, CAST(DATEADD(DAY, 25, @Today) AS datetime2)), 19, 10),
    (19, N'Control de peso y plan de alimentación.', DATEADD(HOUR, 18, CAST(DATEADD(DAY, 20, @Today) AS datetime2)), 11, 18),
    (20, N'Consulta por vómitos y falta de apetito.', DATEADD(HOUR, 9, CAST(DATEADD(DAY, 33, @Today) AS datetime2)), 8, 20),
    (21, N'Evaluación por cojera en pata delantera.', DATEADD(HOUR, 10, CAST(DATEADD(DAY, -100, @Today) AS datetime2)), 2, 15),
    (22, N'Control de peso y plan de alimentación.', DATEADD(HOUR, 11, CAST(DATEADD(DAY, -98, @Today) AS datetime2)), 12, 20),
    (23, N'Análisis de sangre de rutina.', DATEADD(HOUR, 12, CAST(DATEADD(DAY, -160, @Today) AS datetime2)), 2, 13),
    (24, N'Aplicación de vacuna séxtuple.', DATEADD(HOUR, 13, CAST(DATEADD(DAY, -48, @Today) AS datetime2)), 8, 6),
    (25, N'Consulta por picazón y caída de pelo.', DATEADD(HOUR, 14, CAST(DATEADD(DAY, -129, @Today) AS datetime2)), 20, 20),
    (26, N'Consulta por vómitos y falta de apetito.', DATEADD(HOUR, 15, CAST(DATEADD(DAY, -145, @Today) AS datetime2)), 17, 20),
    (27, N'Consulta por tos persistente.', DATEADD(HOUR, 16, CAST(DATEADD(DAY, 31, @Today) AS datetime2)), 1, 7),
    (28, N'Control post operatorio de castración.', DATEADD(HOUR, 17, CAST(DATEADD(DAY, 36, @Today) AS datetime2)), 20, 10),
    (29, N'Análisis de sangre de rutina.', DATEADD(HOUR, 18, CAST(DATEADD(DAY, -21, @Today) AS datetime2)), 2, 6),
    (30, N'Control post operatorio de castración.', DATEADD(HOUR, 9, CAST(DATEADD(DAY, -58, @Today) AS datetime2)), 12, 9),
    (31, N'Análisis de sangre de rutina.', DATEADD(HOUR, 10, CAST(DATEADD(DAY, 47, @Today) AS datetime2)), 12, 19),
    (32, N'Evaluación por cojera en pata delantera.', DATEADD(HOUR, 11, CAST(DATEADD(DAY, -104, @Today) AS datetime2)), 20, 12),
    (33, N'Limpieza dental y control de encías.', DATEADD(HOUR, 12, CAST(DATEADD(DAY, -172, @Today) AS datetime2)), 16, 7),
    (34, N'Consulta por tos persistente.', DATEADD(HOUR, 13, CAST(DATEADD(DAY, 13, @Today) AS datetime2)), 17, 10),
    (35, N'Control post operatorio de castración.', DATEADD(HOUR, 14, CAST(DATEADD(DAY, -37, @Today) AS datetime2)), 14, 11),
    (36, N'Control anual y vacunación antirrábica.', DATEADD(HOUR, 15, CAST(DATEADD(DAY, -136, @Today) AS datetime2)), 16, 20),
    (37, N'Consulta por picazón y caída de pelo.', DATEADD(HOUR, 16, CAST(DATEADD(DAY, -69, @Today) AS datetime2)), 14, 16),
    (38, N'Limpieza dental y control de encías.', DATEADD(HOUR, 17, CAST(DATEADD(DAY, 57, @Today) AS datetime2)), 11, 18),
    (39, N'Revisión de herida en pata trasera.', DATEADD(HOUR, 18, CAST(DATEADD(DAY, -22, @Today) AS datetime2)), 9, 14),
    (40, N'Limpieza dental y control de encías.', DATEADD(HOUR, 9, CAST(DATEADD(DAY, -158, @Today) AS datetime2)), 9, 15);
SET IDENTITY_INSERT [dbo].[Consultations] OFF;

COMMIT TRANSACTION;

PRINT 'Insertados: 5 admins, 15 veterinarios, 85 clientes, 20 mascotas, 40 consultas.';
