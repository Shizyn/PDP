-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Хост: 127.0.0.1:3306
-- Время создания: Июн 04 2025 г., 09:05
-- Версия сервера: 8.0.30
-- Версия PHP: 8.1.9

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `QE`
--

-- --------------------------------------------------------

--
-- Структура таблицы `AvailableSlots`
--

CREATE TABLE `AvailableSlots` (
  `Id` int NOT NULL,
  `ProfProbaId` int NOT NULL,
  `SlotDate` date NOT NULL,
  `TimeRange` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `AvailableSlots`
--

INSERT INTO `AvailableSlots` (`Id`, `ProfProbaId`, `SlotDate`, `TimeRange`) VALUES
(4, 3, '2025-06-05', '13:00-14:00'),
(6, 1, '2025-06-05', '10:00-12:00'),
(7, 1, '2025-06-05', '14:00-16:00'),
(8, 2, '2025-06-05', '10:00-12:00'),
(9, 2, '2025-06-05', '14:00-16:00'),
(10, 3, '2025-06-05', '10:00-12:00'),
(11, 3, '2025-06-05', '14:00-16:00'),
(12, 4, '2025-06-05', '10:00-12:00'),
(13, 4, '2025-06-05', '14:00-16:00'),
(14, 5, '2025-06-05', '10:00-12:00'),
(15, 5, '2025-06-05', '14:00-16:00'),
(16, 6, '2025-06-05', '10:00-12:00'),
(17, 6, '2025-06-05', '14:00-16:00'),
(18, 7, '2025-06-05', '10:00-12:00'),
(19, 7, '2025-06-05', '14:00-16:00');

-- --------------------------------------------------------

--
-- Структура таблицы `Bookings`
--

CREATE TABLE `Bookings` (
  `Id` int NOT NULL,
  `ProfProbaId` int NOT NULL,
  `FullName` varchar(255) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `PhoneNumber` varchar(255) NOT NULL,
  `SchoolName` varchar(255) NOT NULL,
  `BookingDate` date NOT NULL,
  `TimeRange` varchar(255) NOT NULL,
  `Status` enum('Новое','Подтверждено','Отклонено') NOT NULL DEFAULT 'Новое',
  `UserId` int NOT NULL,
  `PeopleCount` int NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `Bookings`
--

INSERT INTO `Bookings` (`Id`, `ProfProbaId`, `FullName`, `Email`, `PhoneNumber`, `SchoolName`, `BookingDate`, `TimeRange`, `Status`, `UserId`, `PeopleCount`) VALUES
(45, 2, 'ааа ааа ааа', 'R@R.ru', '8(222)-222-22-22', 'школа', '2025-06-03', '12:00-14:00', 'Подтверждено', 2, 5),
(61, 3, 'Коленков Илья Захарович', 'zaxar@mail.ru', '8(454)-545-45-45', 'Школа 56', '2025-06-05', '14:00-16:00', 'Новое', 2, 13);

-- --------------------------------------------------------

--
-- Структура таблицы `Events`
--

CREATE TABLE `Events` (
  `Id` int NOT NULL,
  `Name` varchar(255) NOT NULL,
  `ProfProbaId` int NOT NULL,
  `Description` varchar(1000) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `Events`
--

INSERT INTO `Events` (`Id`, `Name`, `ProfProbaId`, `Description`) VALUES
(1, 'Знакомство с оборудованием химической лаборатории по проведению контроля качества литейных изделий', 1, 'апвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывфапвуапвпвыпвыпыввыпывпывф'),
(2, 'Проведение экспертизы и оценка качества литейных изделий', 1, 'укоуеоео'),
(3, 'Знакомство с графическим редактором «Corel Draw»', 2, 'еуоуеуев'),
(4, 'Web-дизайн', 2, 'уевоувоеу'),
(5, 'Знакомство с профессией наладчик КИП', 3, 'оеув'),
(6, 'Конструкция и область применения авиационных приборов', 3, 'оуве'),
(7, 'Экскурсия в музей авиации и космонавтики', 4, 'оуевоувео'),
(8, 'Конструкция авиационных двигателей. Технология испытания газотурбинных двигателей', 4, 'увоуев'),
(9, 'Знакомство с профессией наладчик станков с ЧПУ', 5, 'увеуев'),
(10, 'Демонстрация основных типов станков, их узлов (мультисенсорный комплекс TEKRI)', 5, 'уев'),
(11, 'Разработка управляющих программ для станков с ЧПУ. Обработка деталей с помощью САПР', 5, 'оуевоуыое'),
(12, 'Экспериментальная физика. Проведение лабораторных работ', 5, 'увеоуве'),
(13, 'Художественное литьё в промышленности и архитектурных памятниках', 6, 'еуве'),
(14, 'Получение отливок центробежным литьём. Искусство ювелирного литья', 6, 'уевуве'),
(15, 'Технический контроль в производстве. Специальный и универсальный измерительный инструмент', 7, 'вуевеув'),
(16, 'Проведение контрольно-измерительных работ на ЧПУ-машине', 7, 'веувуев');

-- --------------------------------------------------------

--
-- Структура таблицы `ExcursionBookings`
--

CREATE TABLE `ExcursionBookings` (
  `Id` int NOT NULL,
  `FullName` varchar(255) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `PhoneNumber` varchar(100) NOT NULL,
  `SchoolName` varchar(255) NOT NULL,
  `BookingDate` date NOT NULL,
  `TimeRange` varchar(100) NOT NULL,
  `Status` varchar(100) DEFAULT NULL,
  `UserId` int DEFAULT NULL,
  `PeopleCount` int NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `ExcursionBookings`
--

INSERT INTO `ExcursionBookings` (`Id`, `FullName`, `Email`, `PhoneNumber`, `SchoolName`, `BookingDate`, `TimeRange`, `Status`, `UserId`, `PeopleCount`) VALUES
(18, 'Кочергонов Олег Олегович', 'oleg@mail.ru', '8(454)-545-45-45', 'Школа', '2025-06-06', '12:00-14:00', 'Подтверждена', 2, 15);

-- --------------------------------------------------------

--
-- Структура таблицы `ExcursionSlots`
--

CREATE TABLE `ExcursionSlots` (
  `Id` int NOT NULL,
  `SlotDate` date NOT NULL,
  `TimeRange` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `ExcursionSlots`
--

INSERT INTO `ExcursionSlots` (`Id`, `SlotDate`, `TimeRange`) VALUES
(1, '2025-06-03', '12:00-14:00'),
(2, '2025-06-05', '12:00-14:00'),
(3, '2025-06-05', '10:00-12:00'),
(4, '2025-06-05', '12:00-14:00'),
(5, '2025-06-05', '14:00-16:00'),
(6, '2025-06-06', '10:00-12:00'),
(7, '2025-06-06', '12:00-14:00'),
(8, '2025-06-06', '14:00-16:00'),
(9, '2025-06-09', '10:00-12:00'),
(10, '2025-06-09', '12:00-14:00'),
(11, '2025-06-09', '14:00-16:00'),
(12, '2025-06-10', '10:00-12:00');

-- --------------------------------------------------------

--
-- Структура таблицы `ExcursionUploadedFiles`
--

CREATE TABLE `ExcursionUploadedFiles` (
  `Id` int NOT NULL,
  `ExcursionBookingId` int NOT NULL,
  `FileName` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `FileType` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Content` longblob NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Дамп данных таблицы `ExcursionUploadedFiles`
--

INSERT INTO `ExcursionUploadedFiles` (`Id`, `ExcursionBookingId`, `FileName`, `FileType`, `Content`) VALUES
(3, 18, 'Шаблон_участников(1).xlsx', 'Excel', 0x504b0304140006080800f41ec45ae448adaf1801000033030000130014005b436f6e74656e745f54797065735d2e786d6c9999100000000000000000000000000000000000b592cf4a033110c65f65c9559ab41e44a4db1eaa1e55b03ec098cc7643f38fccb4b66f6f362b22a5821e7a9a24dfccf7fd08335f1ebc6bf698c9c6d08a999c8a06838ec6864d2bded68f935bd1104330e062c0561c91c472315f1f1352536603b5a2674e774a91eed103c9983014a58bd903976bdea8047a0b1b54d7d3e98dd23130069ef0e02116f37bec60e7b8598def83752b2025673570c152c54c340f87228e94c35dfd616e1fcc09cce40b446674b5877a9be8ea34a0a834243c978fc9d6e0bf2262d7598d26ea9d2f2392524630d423b277b256e9c18631f405323f812faeeae0d447ccdbf718b7b26a17011822eaf9b7fc2a92aa65764110e2a3433a47312a978cee21a379e55c96fc3cc1cf866f1055977ef109504b0304140006080800f41ec45a98daeb8bae000000270100000b0014005f72656c732f2e72656c7399991000000000000000000000000000000000008dcfc10e82300c06e057597a978107630c838b31e16af001e6560601d6659b0a6fef8e623c786cfaf7fbd3b25ee6893dd18781ac8022cb81a155a4076b04dcdacbee082c4469b59cc8a2801503d45579c549c67412fac105960c1b04f431ba13e741f538cb9091439b361df959c6347ac39d54a334c8f7797ee0fed380adc91a2dc037ba00d6ae0effb1a9eb068567528f196dfc51f19548b2f406a38065e22ff2e39d68cc120abc2af9e6c1ea0d504b0304140006080800f41ec45a28971ed816030000c20c000018001400786c2f776f726b7368656574732f7368656574312e786d6c99991000000000000000000000000000000000008d97cf6eda4010875fc5da7bb167fc1f0151d255d45eaaaa87f6ecc212ac608c6c13f26e3df491fa0a5d4cb2bb9ec9a68d94003fcf7cb35ee9639d3fbf7e2f6e9e9b7df0a4babe6e0f4b01b34804eab06e37f5e161294ec3f643216e568b73db3df63ba58640971ffa79b714bb6138cec3b05fef5453f5b3f6a80efadab6ed9a6ad01fbb87b0dd6eebb592edfad4a8c31062146561a7f6d5a047f5bbfad88b2bed7f58fdb153d5665c42b3bfa29aaa3e88d562ccbe76815eb3fa52356a294410ae169b5a0fbddc54d0a9ed52dcc25c623e5e191bbed7eadc3bef83cb1dfe6cdbc7cb87cf9ba588c6da9015df8f8bd2f3366a5b9df6c3b7f6fc49d50fbb416f5e3af6acdb7d3ffe0d9afab2a52268aae7f1f55c6f86dd5260348b128ce2049212b2b24414c1fad40f6df3e35a00227400f80240032867904419a6efb6c52f6db169837416435c625cc4699e2671f93e207901241610cf202ff222c35cff446599bd0508af1b30ee96ac866ab5e8da73d05dae6afae5cdadde8b7edc11bd6bbd4e9f56d1227cbab4ea5f5d6d5ad0b4e0d8824e0b9896b1e28e57e0b4e223af88a7159257246f2f2c360b8bc796f8cd25bd5e0b5f574003e904137e62f889c327377497503e0d64e2e1a7869f3a7cb21d7729e5d340a61e7e66f899c34f083fa37c1ac8ccc3cf0d3f77f829e1e7944f03997bf885e1170e3f23fc82f269200b0fbf34fcd2e1e7845f523e0d64e9e14364658b9c090599602e9a112c916e321de2180dce90920e01368426d24da643ec7700a0eb5a44a7209b4213e926d329566898184d9506e6344b24f8ac06ab35b85e03151b98d92c91e0731bacdce0da0d546f607eb34482cf70b08a83eb3850c98159ce12093ecfc18a0eaee9405507e63a4b24f86c07ab3bb8be03151e98f12c91e0731eacf4e05a0f547b60deb34482cf7cb4e6a36b3e50f591a9cf12893ef5d13999919de6a9e734b79261fceff39c97b0039d97b0139d97649ed559397122275d17739325127d6ea275135d37e9170032355922d1a7265a35d15593fa8fcc4c9648f49989d64c74cda4fa23139325129998a1f3f8b8d34ffeaabb6fdb4175e343b9f96f64f517504b0304140006080800f41ec45a70515750da010000570500000d001400786c2f7374796c65732e786d6c9999100000000000000000000000000000000000a5544b6edb3010bd0ac17d23cbfda29014341f01dd74932ebaa5245222407204920ee45cad8b1ea957e890b46439681c17f5c29c797a6f7e1af1f7cf5fc5f5a41579e4d6493025cdaf369470d342274d5fd29d176f3ed1ebaa707eaff8c3c0b927c837aea483f7e3e72c73edc03573573072834f0458cd3cbab6cfdc6839eb5c1069956d379b0f9966d2d0aa303b5d6bef480b3be34b8a29b3aa10608ed05b9a00ccfc441e99c2caf2c8324cf304dc32251b2b239a256efa6f02f2bfb2edebb27838944ba54e0b47a02a46e63db7a646871ceceffb9197d480e1873891f80abdb76c9f6fdfff83c281925da8a3bf050596d8be29695ddfdcdd7dbcbf4f7156da256a3cb09f066c87fb3077b4a53354158a0b1ff456f643343c8ce168c07bd0c1ea24ebc13015b3ccb213795ca592fa21ae42bbae70137fa9c2c09d135d2889e454d3850aa42ec55f2849ec171a3d1838c4962bf51022fe10cb24738c3b0992b6ff6b17173fecd0c10c81d6b21464a57f775e3f8963a09768b8d66c1cd5be8654d0396efe9c1bf6e32408faf141ea7aadb989c8d1ffa2646f349fa7c066970c60e5132608df588b00b769109338dbebd98cf3209719c6899ebc860525e1132fe9b7706fa9d50c9b9d545e9abfbd1a0cda4de2d9ed951d6fc8ea0f504b0304140006080800f41ec45a9994aac9010100007f0100000f001400786c2f776f726b626f6f6b2e786d6c99991000000000000000000000000000000000008d90bd4ec33014855fc5f24e1d18108a927480a512129dd88d73d358f54f74ed50d6c2c073f00688a92a82be82fd24bc024e43042393cffd399f8feed7fba1983f6845ee019db4a6a4a7b38c1230c2d6d2ac4adafbe6e482ceab6263717d67ed9aa46de3722c69eb7d9733e6440b9abb99edc0a4596351739f4a5c31db3452c09515bd06e3d959969d3304c57dfac9b5b27374a4fd87e53a045ebb16c06b35a234978656c590ea56c2c6fd861c4ac2aa82fd991dadd34b0cd750d2f0120e6117b7e133ec497c8acfe1356ee363f808bbb04fcd374a8eeb8b3a1d8612cc6512b8a8931ee81352702596489a5ea9cb246fccb5e5a363682d1184fc39eee89b6256df504b0304140006080800f41ec45a816292a2d6000000340200001a001400786c2f5f72656c732f776f726b626f6f6b2e786d6c2e72656c739999100000000000000000000000000000000000ad91cf6ac3300c875fc5e8be38e9608c51b79731e8b57f1e40d84a1c9ad8c6d2dae5ed6b3656522863879e8464f4fd3eace5fa6b1cd48932f7311868aa1a14051b5d1f3a0387fdc7d32b28160c0e8718c8c0440cebd5724b034a5961df27568511d88017496f5ab3f53422573151282f6dcc234a6973a713da2376a41775fda2f39c01b74cb57106f2c635a0f653a2ffb063dbf696dea3fd1c29c89d087d8ef9c89e480a14734762e03a62fd5d9aaa5041df97593c5286651aca5f5e4d7efabfe29f1f1aef3193db492e879e5bccc7bf32fae6daab0b504b0304140006080800f41ec45a33bbb640190100009901000014001400786c2f736861726564537472696e67732e786d6c99991000000000000000000000000000000000006d90c14ac43010865f25e4ac9bea4145daee41f009f401421bb78536a94d2a7ab3ab2082e845c493b0e0c16359dc65d9e2f61526afe00bf80a4eab22586f337ffef9fec97cd48d3b3c4d137222721d2be9d18d81438990810a6339f2e8e1c1fefa0e25da7019f24449e1d133a12919faaed686e0a8d41e8d8cc97619d3412452ae072a13125f8e549e72836d3e623acb050f7524844913b6e9385b2ce5b1a4245085341edda6a490f17121f67e7a0c887dd7f830810616b684152c89bdb05750d9d28ee10d16b04471ea32e3bbac357f0dbc5f3efc95e0191ee1a9a7de236a0c15b1e7c899c32bcc5aaabdeb195fbaa41aaa358255dd6d50f65c13f4341d6bfacdabecadbdc63d67ff44b7bc39e6addadfd89b5f07c3c3fa9f504b01022d00140006080800f41ec45ae448adaf18010000330300001300000000000000000000000000000000005b436f6e74656e745f54797065735d2e786d6c504b01022d00140006080800f41ec45a98daeb8bae000000270100000b000000000000000000000000005d0100005f72656c732f2e72656c73504b01022d00140006080800f41ec45a28971ed816030000c20c0000180000000000000000000000000048020000786c2f776f726b7368656574732f7368656574312e786d6c504b01022d00140006080800f41ec45a70515750da010000570500000d00000000000000000000000000a8050000786c2f7374796c65732e786d6c504b01022d00140006080800f41ec45a9994aac9010100007f0100000f00000000000000000000000000c1070000786c2f776f726b626f6f6b2e786d6c504b01022d00140006080800f41ec45a816292a2d6000000340200001a0000000000000000000000000003090000786c2f5f72656c732f776f726b626f6f6b2e786d6c2e72656c73504b01022d00140006080800f41ec45a33bbb64019010000990100001400000000000000000000000000250a0000786c2f736861726564537472696e67732e786d6c504b05060000000007000700c2010000840b00000000);

-- --------------------------------------------------------

--
-- Структура таблицы `ProfProby`
--

CREATE TABLE `ProfProby` (
  `Id` int NOT NULL,
  `Name` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `ProfProby`
--

INSERT INTO `ProfProby` (`Id`, `Name`) VALUES
(1, 'Металлургическое производство (по видам производства)'),
(2, 'Информационные системы и программирование'),
(3, 'Авиационные приборы и комплексы'),
(4, 'Производство авиационных двигателей'),
(5, 'Технология машиностроения'),
(6, 'Металлургическое производство (художественное литьё)'),
(7, 'Управление качеством продукции, процессов и услуг');

-- --------------------------------------------------------

--
-- Структура таблицы `UploadedFiles`
--

CREATE TABLE `UploadedFiles` (
  `Id` int NOT NULL,
  `BookingId` int NOT NULL,
  `FileName` varchar(255) NOT NULL,
  `FileType` varchar(50) NOT NULL,
  `Content` longblob NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `UploadedFiles`
--

INSERT INTO `UploadedFiles` (`Id`, `BookingId`, `FileName`, `FileType`, `Content`) VALUES
(17, 61, 'Шаблон_участников (4)(1).xlsx', 'Excel', 0x504b0304140006080800ec88ba5ae448adaf1801000033030000130014005b436f6e74656e745f54797065735d2e786d6c9999100000000000000000000000000000000000b592cf4a033110c65f65c9559ab41e44a4db1eaa1e55b03ec098cc7643f38fccb4b66f6f362b22a5821e7a9a24dfccf7fd08335f1ebc6bf698c9c6d08a999c8a06838ec6864d2bded68f935bd1104330e062c0561c91c472315f1f1352536603b5a2674e774a91eed103c9983014a58bd903976bdea8047a0b1b54d7d3e98dd23130069ef0e02116f37bec60e7b8598def83752b2025673570c152c54c340f87228e94c35dfd616e1fcc09cce40b446674b5877a9be8ea34a0a834243c978fc9d6e0bf2262d7598d26ea9d2f2392524630d423b277b256e9c18631f405323f812faeeae0d447ccdbf718b7b26a17011822eaf9b7fc2a92aa65764110e2a3433a47312a978cee21a379e55c96fc3cc1cf866f1055977ef109504b0304140006080800ec88ba5a98daeb8bae000000270100000b0014005f72656c732f2e72656c7399991000000000000000000000000000000000008dcfc10e82300c06e057597a978107630c838b31e16af001e6560601d6659b0a6fef8e623c786cfaf7fbd3b25ee6893dd18781ac8022cb81a155a4076b04dcdacbee082c4469b59cc8a2801503d45579c549c67412fac105960c1b04f431ba13e741f538cb9091439b361df959c6347ac39d54a334c8f7797ee0fed380adc91a2dc037ba00d6ae0effb1a9eb068567528f196dfc51f19548b2f406a38065e22ff2e39d68cc120abc2af9e6c1ea0d504b0304140006080800ec88ba5a28971ed816030000c20c000018001400786c2f776f726b7368656574732f7368656574312e786d6c99991000000000000000000000000000000000008d97cf6eda4010875fc5da7bb167fc1f0151d255d45eaaaa87f6ecc212ac608c6c13f26e3df491fa0a5d4cb2bb9ec9a68d94003fcf7cb35ee9639d3fbf7e2f6e9e9b7df0a4babe6e0f4b01b34804eab06e37f5e161294ec3f643216e568b73db3df63ba58640971ffa79b714bb6138cec3b05fef5453f5b3f6a80efadab6ed9a6ad01fbb87b0dd6eebb592edfad4a8c31062146561a7f6d5a047f5bbfad88b2bed7f58fdb153d5665c42b3bfa29aaa3e88d562ccbe76815eb3fa52356a294410ae169b5a0fbddc54d0a9ed52dcc25c623e5e191bbed7eadc3bef83cb1dfe6cdbc7cb87cf9ba588c6da9015df8f8bd2f3366a5b9df6c3b7f6fc49d50fbb416f5e3af6acdb7d3ffe0d9afab2a52268aae7f1f55c6f86dd5260348b128ce2049212b2b24414c1fad40f6df3e35a00227400f80240032867904419a6efb6c52f6db169837416435c625cc4699e2671f93e207901241610cf202ff222c35cff446599bd0508af1b30ee96ac866ab5e8da73d05dae6afae5cdadde8b7edc11bd6bbd4e9f56d1227cbab4ea5f5d6d5ad0b4e0d8824e0b9896b1e28e57e0b4e223af88a7159257246f2f2c360b8bc796f8cd25bd5e0b5f574003e904137e62f889c327377497503e0d64e2e1a7869f3a7cb21d7729e5d340a61e7e66f899c34f083fa37c1ac8ccc3cf0d3f77f829e1e7944f03997bf885e1170e3f23fc82f269200b0fbf34fcd2e1e7845f523e0d64e9e14364658b9c090599602e9a112c916e321de2180dce90920e01368426d24da643ec7700a0eb5a44a7209b4213e926d329566898184d9506e6344b24f8ac06ab35b85e03151b98d92c91e0731bacdce0da0d546f607eb34482cf70b08a83eb3850c98159ce12093ecfc18a0eaee9405507e63a4b24f86c07ab3bb8be03151e98f12c91e0731eacf4e05a0f547b60deb34482cf7cb4e6a36b3e50f591a9cf12893ef5d13999919de6a9e734b79261fceff39c97b0039d97b0139d97649ed559397122275d17739325127d6ea275135d37e9170032355922d1a7265a35d15593fa8fcc4c9648f49989d64c74cda4fa23139325129998a1f3f8b8d34ffeaabb6fdb4175e343b9f96f64f517504b0304140006080800ec88ba5a70515750da010000570500000d001400786c2f7374796c65732e786d6c9999100000000000000000000000000000000000a5544b6edb3010bd0ac17d23cbfda29014341f01dd74932ebaa5245222407204920ee45cad8b1ea957e890b46439681c17f5c29c797a6f7e1af1f7cf5fc5f5a41579e4d6493025cdaf369470d342274d5fd29d176f3ed1ebaa707eaff8c3c0b927c837aea483f7e3e72c73edc03573573072834f0458cd3cbab6cfdc6839eb5c1069956d379b0f9966d2d0aa303b5d6bef480b3be34b8a29b3aa10608ed05b9a00ccfc441e99c2caf2c8324cf304dc32251b2b239a256efa6f02f2bfb2edebb27838944ba54e0b47a02a46e63db7a646871ceceffb9197d480e1873891f80abdb76c9f6fdfff83c281925da8a3bf050596d8be29695ddfdcdd7dbcbf4f7156da256a3cb09f066c87fb3077b4a53354158a0b1ff456f643343c8ce168c07bd0c1ea24ebc13015b3ccb213795ca592fa21ae42bbae70137fa9c2c09d135d2889e454d3850aa42ec55f2849ec171a3d1838c4962bf51022fe10cb24738c3b0992b6ff6b17173fecd0c10c81d6b21464a57f775e3f8963a09768b8d66c1cd5be8654d0396efe9c1bf6e32408faf141ea7aadb989c8d1ffa2646f349fa7c066970c60e5132608df588b00b769109338dbebd98cf3209719c6899ebc860525e1132fe9b7706fa9d50c9b9d545e9abfbd1a0cda4de2d9ed951d6fc8ea0f504b0304140006080800ec88ba5a9994aac9010100007f0100000f001400786c2f776f726b626f6f6b2e786d6c99991000000000000000000000000000000000008d90bd4ec33014855fc5f24e1d18108a927480a512129dd88d73d358f54f74ed50d6c2c073f00688a92a82be82fd24bc024e43042393cffd399f8feed7fba1983f6845ee019db4a6a4a7b38c1230c2d6d2ac4adafbe6e482ceab6263717d67ed9aa46de3722c69eb7d9733e6440b9abb99edc0a4596351739f4a5c31db3452c09515bd06e3d959969d3304c57dfac9b5b27374a4fd87e53a045ebb16c06b35a234978656c590ea56c2c6fd861c4ac2aa82fd991dadd34b0cd750d2f0120e6117b7e133ec497c8acfe1356ee363f808bbb04fcd374a8eeb8b3a1d8612cc6512b8a8931ee81352702596489a5ea9cb246fccb5e5a363682d1184fc39eee89b6256df504b0304140006080800ec88ba5a816292a2d6000000340200001a001400786c2f5f72656c732f776f726b626f6f6b2e786d6c2e72656c739999100000000000000000000000000000000000ad91cf6ac3300c875fc5e8be38e9608c51b79731e8b57f1e40d84a1c9ad8c6d2dae5ed6b3656522863879e8464f4fd3eace5fa6b1cd48932f7311868aa1a14051b5d1f3a0387fdc7d32b28160c0e8718c8c0440cebd5724b034a5961df27568511d88017496f5ab3f53422573151282f6dcc234a6973a713da2376a41775fda2f39c01b74cb57106f2c635a0f653a2ffb063dbf696dea3fd1c29c89d087d8ef9c89e480a14734762e03a62fd5d9aaa5041df97593c5286651aca5f5e4d7efabfe29f1f1aef3193db492e879e5bccc7bf32fae6daab0b504b0304140006080800ec88ba5a33bbb640190100009901000014001400786c2f736861726564537472696e67732e786d6c99991000000000000000000000000000000000006d90c14ac43010865f25e4ac9bea4145daee41f009f401421bb78536a94d2a7ab3ab2082e845c493b0e0c16359dc65d9e2f61526afe00bf80a4eab22586f337ffef9fec97cd48d3b3c4d137222721d2be9d18d81438990810a6339f2e8e1c1fefa0e25da7019f24449e1d133a12919faaed686e0a8d41e8d8cc97619d3412452ae072a13125f8e549e72836d3e623acb050f7524844913b6e9385b2ce5b1a4245085341edda6a490f17121f67e7a0c887dd7f830810616b684152c89bdb05750d9d28ee10d16b04471ea32e3bbac357f0dbc5f3efc95e0191ee1a9a7de236a0c15b1e7c899c32bcc5aaabdeb195fbaa41aaa358255dd6d50f65c13f4341d6bfacdabecadbdc63d67ff44b7bc39e6addadfd89b5f07c3c3fa9f504b01022d00140006080800ec88ba5ae448adaf18010000330300001300000000000000000000000000000000005b436f6e74656e745f54797065735d2e786d6c504b01022d00140006080800ec88ba5a98daeb8bae000000270100000b000000000000000000000000005d0100005f72656c732f2e72656c73504b01022d00140006080800ec88ba5a28971ed816030000c20c0000180000000000000000000000000048020000786c2f776f726b7368656574732f7368656574312e786d6c504b01022d00140006080800ec88ba5a70515750da010000570500000d00000000000000000000000000a8050000786c2f7374796c65732e786d6c504b01022d00140006080800ec88ba5a9994aac9010100007f0100000f00000000000000000000000000c1070000786c2f776f726b626f6f6b2e786d6c504b01022d00140006080800ec88ba5a816292a2d6000000340200001a0000000000000000000000000003090000786c2f5f72656c732f776f726b626f6f6b2e786d6c2e72656c73504b01022d00140006080800ec88ba5a33bbb64019010000990100001400000000000000000000000000250a0000786c2f736861726564537472696e67732e786d6c504b05060000000007000700c2010000840b00000000);

-- --------------------------------------------------------

--
-- Структура таблицы `Users`
--

CREATE TABLE `Users` (
  `UserId` int NOT NULL,
  `FullName` varchar(255) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `Phone` varchar(255) NOT NULL,
  `Password` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `Users`
--

INSERT INTO `Users` (`UserId`, `FullName`, `Email`, `Phone`, `Password`) VALUES
(1, 'Ротанова Дарья Валерьевна', 'rotanovad@bk.ru', '88005553535', 'asdfg'),
(2, 'Иванов Иван Иванович', 'ivanov@mail.ru', '8(912)-512-01-92', 'haha0'),
(6, 'Синицина Ольга Владимировна', 'cinic@yandex.ru', '8(912)-848-47-47', 'cin20'),
(7, 'Ощепков Михаил Арутюновичь', 'ivanov@mail.ru', '8(926)-727-85-97', 'haha0'),
(8, 'Ощепков Михаил Арутюновичь', 'ivanov@mail.ru', '8(926)-727-85-97', 'haha0'),
(9, 'Осипов Роман', 'ivanov@mail.ru', '8(982)-439-90-75', 'haha0'),
(10, 'Ощепков Михаил Арутюновичь', 'ivanov@mail.ru', '8(926)-727-85-97', 'haha0');

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `AvailableSlots`
--
ALTER TABLE `AvailableSlots`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `proba` (`ProfProbaId`);

--
-- Индексы таблицы `Bookings`
--
ALTER TABLE `Bookings`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `KvantumID` (`ProfProbaId`),
  ADD KEY `bookings_ibfk_4_idx` (`UserId`);

--
-- Индексы таблицы `Events`
--
ALTER TABLE `Events`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `FK_Events_Kvantums` (`ProfProbaId`);

--
-- Индексы таблицы `ExcursionBookings`
--
ALTER TABLE `ExcursionBookings`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `UserId` (`UserId`);

--
-- Индексы таблицы `ExcursionSlots`
--
ALTER TABLE `ExcursionSlots`
  ADD PRIMARY KEY (`Id`);

--
-- Индексы таблицы `ExcursionUploadedFiles`
--
ALTER TABLE `ExcursionUploadedFiles`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_ExcursionUploadedFiles_ExcursionBookingId` (`ExcursionBookingId`);

--
-- Индексы таблицы `ProfProby`
--
ALTER TABLE `ProfProby`
  ADD PRIMARY KEY (`Id`);

--
-- Индексы таблицы `UploadedFiles`
--
ALTER TABLE `UploadedFiles`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `FK_UploadedFiles_Bookings` (`BookingId`);

--
-- Индексы таблицы `Users`
--
ALTER TABLE `Users`
  ADD PRIMARY KEY (`UserId`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `AvailableSlots`
--
ALTER TABLE `AvailableSlots`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=20;

--
-- AUTO_INCREMENT для таблицы `Bookings`
--
ALTER TABLE `Bookings`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=63;

--
-- AUTO_INCREMENT для таблицы `Events`
--
ALTER TABLE `Events`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=43;

--
-- AUTO_INCREMENT для таблицы `ExcursionBookings`
--
ALTER TABLE `ExcursionBookings`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT для таблицы `ExcursionSlots`
--
ALTER TABLE `ExcursionSlots`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT для таблицы `ExcursionUploadedFiles`
--
ALTER TABLE `ExcursionUploadedFiles`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT для таблицы `ProfProby`
--
ALTER TABLE `ProfProby`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT для таблицы `UploadedFiles`
--
ALTER TABLE `UploadedFiles`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT для таблицы `Users`
--
ALTER TABLE `Users`
  MODIFY `UserId` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `AvailableSlots`
--
ALTER TABLE `AvailableSlots`
  ADD CONSTRAINT `proba` FOREIGN KEY (`ProfProbaId`) REFERENCES `ProfProby` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

--
-- Ограничения внешнего ключа таблицы `Bookings`
--
ALTER TABLE `Bookings`
  ADD CONSTRAINT `bookings_ibfk_2` FOREIGN KEY (`ProfProbaId`) REFERENCES `ProfProby` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  ADD CONSTRAINT `bookings_ibfk_4` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`);

--
-- Ограничения внешнего ключа таблицы `Events`
--
ALTER TABLE `Events`
  ADD CONSTRAINT `FK_Events_Kvantums` FOREIGN KEY (`ProfProbaId`) REFERENCES `ProfProby` (`Id`);

--
-- Ограничения внешнего ключа таблицы `ExcursionUploadedFiles`
--
ALTER TABLE `ExcursionUploadedFiles`
  ADD CONSTRAINT `FK_ExcursionUploadedFiles_ExcursionBookings_ExcursionBookingId` FOREIGN KEY (`ExcursionBookingId`) REFERENCES `ExcursionBookings` (`Id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `UploadedFiles`
--
ALTER TABLE `UploadedFiles`
  ADD CONSTRAINT `FK_UploadedFiles_Bookings` FOREIGN KEY (`BookingId`) REFERENCES `Bookings` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
